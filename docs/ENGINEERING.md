# TutoringHub — Engineering Case Study

> How the private-tutoring economy of Egypt shaped a Clean Architecture system in ASP.NET Core 8 + React 19.

For setup and a feature overview, see the [README](README.md).

---

## 1. Business Analysis

Private tutoring in Egypt runs on a few fixed facts: students pay **after** lessons, one by one; a new month means a fresh accounting page; a missed class is "made up" by joining another group covering the same material; and no student is ever turned away over money in the moment. The domain model mirrors those facts directly:

| Entity | Real-world meaning | Key design |
|---|---|---|
| `Teacher` | The center's owner-operator | Username + phone, unique; BCrypt hash; single teacher per center |
| `Center` | The physical building where groups meet | Each center owned by one teacher |
| `ClassGroup` | A recurring class (day, start time, weekly frequency) | `DayOfWeek`, `TimeSpan StartTime`, `Frequency` enum |
| `ClassSession` | One real meeting of a group, on a real date | `DateOnly`; UNIQUE `(ClassGroupId, Date)` |
| `Student` | A learner who attends sessions | Logs in with **phone + 4-digit PIN**, nothing else |
| `Enrollment` | The link between a student and their group | UNIQUE `(StudentId, ClassGroupId)` |
| `Attendance` | The single most important fact: were they there? | UNIQUE `(ClassSessionId, StudentId)`; `QuotaRowId` nullable → paid vs unpaid |
| `QuotaRow` | "8 sessions bought for 400 EGP for this group this month" | Append-only; `TotalSessions`, `RemainingSessions`, `Price`, `PeriodStart/End`, `PaidAt` |
| `Quiz` / `QuizQuestion` / `QuizAttempt` | Teacher-made drills + student attempts | Draft → published → assigned to a class group |
| `RefreshToken` | Long-lived session key | One table + `Role` column; SHA-256 hash at rest |

The single most consequential modeling decision: **attendance is the source of truth, and quota is only accounting**. Most billing systems treat money as truth and attendance as a side effect. Tutoring centers work the opposite way — the teacher marks who actually came, and the money has to follow that fact, not the other way around. So `Attendance` never derives from a paid quota, and a quota never gates attendance: an unpaid student attends, and the row is simply marked unpaid. That one decision makes tickets, make-up, and month boundaries all solve themselves consistently.

---

## 2. Architecture

Strict Clean Architecture, enforced by project references:

```
TutoringHub.Domain          → nothing
TutoringHub.Application     → Domain
TutoringHub.Infrastructure  → Application, Domain
TutoringHub.API             → Infrastructure
```

- **Controllers are thin** — `[Authorize(Roles=...)]`, read the tenant from `ICurrentUserService` (JWT claims), call exactly one service method, return a DTO. The `AttendanceController` is 39 lines and does nothing but route.
- **Services own the rules**, take DTOs, return DTOs, never leak `IQueryable`. Failures are plain exceptions (`KeyNotFoundException` → 404, `ArgumentException` → 400, `UnauthorizedAccessException` → 401, AI failures → 502) translated by the global exception handler.
- **15 validators** (one per request shape) auto-register and auto-apply via SharpGrip `AddFluentValidationAutoValidation()`; validation short-circuits before controller bodies.
- **Frontend** is a React 19 SPA (JSX) with Teacher and Student layouts, an Axios client that attaches the Bearer token and refreshes on 401, `i18next` AR/EN with an automatic `dir` flip, and dark mode.

### Why no repository pattern?

A common Clean Architecture example wraps EF Core in a generic `IRepository<T>`. This project doesn't. `IApplicationDbContext` (in Domain, exposing `DbSet<T>` + `SaveChangesAsync`) is the abstraction the Application layer depends on, and EF's `DbSet<T>` plus change tracking already behaves like a repository with a unit of work. Adding a repository wrapper would add pass-through code without adding testability that isn't already there — the test suite runs services directly against an InMemory `IApplicationDbContext`. MediatR/CQRS and microservices were rejected for the same reason: no problem at this scale needs them.

Also deliberately single-tenanted *per teacher*: one teacher per center, and every service resolves the current `TeacherId` from JWT claims — **never** from URL or body. A student's request to `GET /api/me/quotas` can't ask for another teacher's data because the tenant isn't something the caller controls.

---

## 3. Request Lifecycle

`POST /api/classes/{id}/attendance` (ticking a student present) as the representative path:

```
Browser → Bearer token → [Authorize(Roles="Teacher")]
  → FluentValidation (TickAttendanceRequest)
  → AttendanceController.Tick
  → AttendanceService.TickAsync
     ├─ resolve student enrollment / session (teacher-scoped query)
     ├─ derive IsMakeUp: ticked session group ≠ student's enrolled group?
     ├─ consume quota: FIFO — oldest unpaid QuotaRow → RemainingSessions -= 1
     │   (or leave QuotaRowId null if no quota covers it — unpaid, never blocked)
     └─ one SaveChangesAsync: Attendance row + quota decrement atomically
  → 200 AttendanceEntryDto (isPresent, isMakeUp, remainingSessions)
```

`DELETE .../attendance/{studentId}?date=` is the inverse: one `SaveChanges` that removes the attendance row *and* increments the same quota column back — symmetric reversal, no drift possible.

Read paths (rosters, sessions, lists) are teacher-scoped lookups returning DTOs; heavy dashboard reads are not a bottleneck at this scale, so no premature caching.

---

## 4. Authentication & Authorization

Two very different identities share one JWT pipeline:

- **Teacher** — username + password, BCrypt-verified. One teacher runs the center.
- **Student** — phone + **4-digit PIN**. No email, no password alphabet — the reality of the audience (children, parents sharing one phone). PIN is stored BCrypt-hashed and regex-validated `^\d{4}$`.

Beyond the access token, there's a **refresh-token flow** — the piece most MVC tutorial stacks skip:

- `POST /api/auth/refresh` rotates the token: the presented hash is validated, revoked, and a new token issued (one table, `Role` column so students and teachers share it).
- At rest, tokens are stored **SHA-256 hashed**, never plaintext — a DB leak isn't a session-keys leak.
- **Replay detection**: a refresh token used twice returns `401 "already been used"` — rotation means the old one is dead the moment it's redeemed once; a reused token is the classic token-theft signal.

Auth endpoints are rate-limited **5/min per IP**; AI endpoints get their own budget. All of this is centralized in `AddJwtAuthentication` + `AddRateLimitingPolicy` — a new tenant flow can't forget to add security because security is in the pipeline, not per-endpoint.

---

## 5. Key Business Rules (and why)

| Rule | Why it exists |
|---|---|
| Attendance is truth; quota is accounting | A center's books are about who actually came. Money follows attendance, never the reverse |
| Postpaid auto-FIFO: new quota covers oldest unpaid sessions first | A payment in the middle of the month must pay for the debt that's been building, not the sessions ahead — the "fresh page" habit, made automatic |
| Quota rows are append-only; new month = new row | History is evidence; editing a past row corrupts make-up/undo math and audits |
| Make-up consumes the student's **own** group quota | A make-up is the same material delivered once — it can't consume a group the student never subscribed to |
| `IsMakeUp` is derived, never stored | Storing it invites conflicting flags; deriving it from the enrollment diff keeps one source of truth |
| Undo reverses consumption symmetrically | Un-ticking must put back exactly what ticking took — one atomic `SaveChanges`, no compensation steps |
| UNIQUE `(ClassGroupId, Date)` and `(ClassSessionId, StudentId)` | Race-safety at the DB level: two tabs can't create the same session or double-tick a student |
| One teacher per center; tenant always from JWT claims | A caller can't read or write another teacher's data by tampering with a URL or body field — tenancy is uncallable |

The FIFO rule is the one worth spelling out, because it's reflexive and surprising:

- Student buys 8 sessions on the 10th → newest `QuotaRow` has `RemainingSessions = 8`.
- Missed attendance on the 12th and 15th (unpaid), attends the 19th.
- The 19th tick consumes the *oldest* session debt first. If the first payment has already fully run out, it starts consuming the second row — but as a date-stamped chain, not a live balance. Nothing is "reset" by the calendar.

---

## 6. Attendance & Billing Together

Robustness comes from keeping those two entities **loosely coupled by design**: `Attendance.QuotaRowId` is nullable and set *opportunistically* at tick time. That means:

- A student who has never paid still gets ticked present (`QuotaRowId = null`). The teacher's roster shows exactly the unpaid gap — an honest accounting surface, enforced by the domain.
- When money arrives via `POST /api/students/{id}/quotas/{quotaId}/pay`, it's simply marking a row paid; the FIFO *coverage* of unpaid attendances is recomputed/represented on read — the closest thing to "getting paid" that still treats attendance as the source.
- Undo is therefore safe in every state: un-ticking a paid-session attendance restores the quota column; un-ticking an unpaid one touches nothing but the attendance row.

---

## 7. AI Integration (human-in-the-loop)

Quiz generation is Google Gemini via a server-side key (students can never read it — generation is not a client call). The flow is intentionally **two-step**:

1. `POST /api/ai/generate` — teacher submits subject/class/groups/topic/count/question-type; the service asks Gemini for a strict-JSON payload matching the `QuizQuestion` shape.
2. The result is returned as a **draft** to the review UI. Nothing enters the database until `POST /api/quizzes` is explicitly called — a human approves the content. No silent writes, no generated-and-persisted surprises.

On the client side the generator retries on transient provider failures (429/503), maps AI errors to readable messages, and never stores the API key anywhere a response could expose it (`AiClientException` → 502). The odd-but-real constraint — pinned Microsoft packages at `8.0.0`, third-party packages left at their own versions — keeps the published output lean and predictable.

---

## 8. Testing Strategy

141 xUnit tests (xUnit + FluentAssertions + EF Core InMemory via a shared `MockDbContext`), no test flakiness, no external services:

- **Service tests** — attendance tick/untick with FIFO consumption *and* symmetric undo, make-up coverage consuming the own-group quota, quota pay, enrollments (duplicate rejection), auth including refresh-token rotation and **replay → 401**, quiz publish/unpublish gates.
- **Validator tests** — all 15 request shapes, including PIN format and teacher/student login rules.
- **A DI smoke test** that resolves every registered service — it literally fails the build if any service forgets its `AddScoped`. This is the cheap guard against the most common real-world bug in this codebase shape (a new service, a forgotten registration, a runtime-only exception).

Two hard-won testing lessons are worth capturing:

1. **The InMemory provider emulates INNER JOIN on required relationships.** A dependent whose FK points at a non-existent principal (set from a *foreign-key value captured before save*) is silently dropped. Test graphs must be built via **navigation references** (`Attendance { ClassSession = session, Student = student }`) so EF fixes up identity, not copied ints.
2. **Write the seed once, treat it as a contract.** The demo seed (`SeedDemoDataAsync`) is idempotent and gated by `SeedDemo:Enabled` — it's not just convenience, it's the source of truth the live demo, the smoke flows, and these docs all depend on. Changing it is a breaking change by intent.

**Not yet covered**, and next on the list: integration tests against a real SQL Server, controller-level API tests, and frontend component tests.

---

## 9. Deployment

```
push to main
     │
     ├──► GitHub Actions
     │       backend: restore → build → test 141 (ubuntu)
     │       frontend: npm ci → lint → build  (ubuntu)
     │       deploy:  needs[backend, frontend] (windows-latest)
     │                dotnet publish → msdeploy sync → MonsterASP.NET
     │
     └──► Vercel: vite build → global CDN (VITE_API_URL env)
```

- Deploy is **gated by the test job** — a red suite never reaches production.
- Web Deploy is driven by a **single `MONSTER_PUBLISH_PROFILE` GitHub secret** — one paste, zero field-mapping errors (chosen over multi-secret setups after comparing the sibling projects).
- `Migrate()` + idempotent seeding run on **API startup**, so a fresh deploy provisions the database with no manual SQL.
- Production secrets are **environment variables only**: the connection string, `Jwt:Key`, the Gemini key, and `FrontendUrl` (CORS). The committed config carries no secrets.
- **CORS incident, caught live:** the first production rollout returned no `Access-Control-Allow-Origin` for the Vercel origin. Diagnosed with preflight probes against the live endpoint, it turned out to be a missing origin in config, not a code bug — fixed by merging the `FrontendUrl` env var into the policy at startup with a commit, redeployed green the same hour.
- **`.pubxml` attribute lesson:** the Monster publish profile is *attribute-based* (`publishData`/`publishProfile` with `publishUrl`, `msdeploySite`, `userName`, `userPWD` as attributes) — not element-based like the Visual Studio default. The deploy job parses it as XML using that shape; assuming the textbook element format had failed the first deploy. The endpoint is `https://site91247.siteasp.net:8172/msdeploy.axd?site=site91247`.

---

## 10. Engineering Decisions

| Decision | Rationale |
|---|---|
| Attendance-first domain, quota as accounting | Money-as-truth products mis-model this business and produce honest books only by accident |
| FIFO quota rows, append-only | Reflects how centers actually reconcile: new payment → oldest debt first, month = new row |
| `IsMakeUp` derived, never stored | One source of truth; enrollment diff is provable at read time |
| Nullable `QuotaRowId` on Attendance | Blocks nothing over money; drives the unpaid/paid surface |
| JWT-claim tenancy (`ICurrentUserService`) | Multi-tenant safety without multi-tenant complexity; nothing tenant-ish is caller-supplied |
| Refresh tokens: rotation + SHA-256 + replay→401 | Long-lived sessions without the token-theft hole most tutorials ship |
| `IApplicationDbContext`, no repository pattern | EF `DbSet` + change tracking already is the repository/UoW; the abstraction is in the right layer |
| Student login = phone + PIN | Matches the audience — no email, no keyboard, shared phones |
| Single `MONSTER_PUBLISH_PROFILE` secret | One-paste deploy beats 4-secret field mapping |
| Human-in-the-loop AI | Generation drafts; a teacher's review is the gate before persistence |
| Auto-validation (SharpGrip) + DI smoke test | Security and wiring mistakes become compile-time, not runtime |

---

## 11. Lessons Learned

1. **Model the operator's day, not the CRUD.** "Course = product, payment = truth" is how generic apps model tutoring, and it's wrong for centers. The whole system got simpler once attendance became the fact and quota became accounting.
2. **Derived state beats stored state.** `IsMakeUp`, `RemainingSessions`-vs-attendance coverage, month boundaries — all computed from one source of truth. A flag you store is a lie you'll have to reconcile.
3. **The refresh token is where auth leaks.** Plaintext storage, no rotation, no replay check — every one of those is a real breach in production. Cheap to do right, invisible until it isn't.
4. **InMemory testing has its own physics.** The INNER-JOIN emulation rule (use navigation references, never pre-save FK values) was learned the hard way — a silently-dropped graph assumption in the test helper cost an afternoon.
5. **Seed data is a contract.** The demo seed is load-bearing: live demo, smoke flows, and this document all assume `admin/admin123` + the demo students. Treat seed changes like schema migrations.
6. **Deploy is part of the engineering.** The CORS origin and the attribute-based `.pubxml` both bit at rollout, not in code review — preflight probing and reading the actual publish file parsed as XML were the fixes.