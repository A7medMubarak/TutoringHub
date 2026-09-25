# 🏫 TutoringHub

> A production-ready Tutoring Center Management System built with **ASP.NET Core 8**, **React 19**, and **Clean Architecture** — where attendance is the truth, quota is accounting, and private-lesson billing follows the way tutoring centers in Egypt actually work.

<p align="center">

[🌐 Live](https://tutoring-hub-psi.vercel.app)
•
[📚 Engineering Case Study](docs/ENGINEERING.md)

</p>

---

![.NET](https://img.shields.io/badge/.NET-8-512BD4?style=for-the-badge&logo=dotnet)
![React](https://img.shields.io/badge/React-19-61DAFB?style=for-the-badge&logo=react)
![JavaScript](https://img.shields.io/badge/JavaScript-JSX-f7df1e?style=for-the-badge&logo=javascript)
![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge)
![JWT](https://img.shields.io/badge/JWT-Refresh_Tokens-black?style=for-the-badge)
![Clean Architecture](https://img.shields.io/badge/Clean_Architecture-blue?style=for-the-badge)
![GitHub Actions](https://img.shields.io/badge/CI/CD-GitHub_Actions-2088FF?style=for-the-badge)
![Tests](https://img.shields.io/badge/Tests-141_Passing-success?style=for-the-badge)

---

## 📸 Preview

| Dashboard | Classes | Students |
|-----------|----------|----------|
| <img src="docs/screenshots/dashboard.png" width="300" alt="Dashboard"> | <img src="docs/screenshots/classes.png" width="300" alt="Classes"> | <img src="docs/screenshots/students.png" width="300" alt="Students"> |

| Attendance | Quizzes | New Quiz (AI draft) |
|------------|---------|---------------------|
| <img src="docs/screenshots/attendance.png" width="300" alt="Attendance"> | <img src="docs/screenshots/quizzes.png" width="300" alt="Quizzes"> | <img src="docs/screenshots/newQuiz.png" width="300" alt="New Quiz"> |

---

# Why This Project?

This project is the third in a series where I started from the **business**, not the framework. I don't build an app and then look for a problem to put it in — I work inside the business first, learn what the operators actually do every day, and only then design the system around those rules.

- **HotelManager** came from **2 years as a hotel receptionist** — night audits, double-booking, guest balances, business days.
- **PaintShop** came from **5 years behind a GLC paints tinting counter** — shop vs warehouse stock, per-line discounts, cancellation restoring the shelf.
- **TutoringHub** comes from my **Education degree in French Literature at Al-Arish University**, and from staying connected to the teachers and students who live the private-lesson economy.

Private tutoring in Egypt is huge, paid **attendance-by-attendance**, and run on notebooks. A teacher runs class groups in a physical center, students pay monthly or per-attendance, missed classes are made up by joining another session, and "who has paid for how many lessons this month" is a conversation, not a system. Most off-the-shelf apps model tutoring the wrong way — they treat a "course" as a flat product and money as the source of truth. The moment a teacher counts attendance as revenue that wasn't actually delivered, the books lie.

So TutoringHub models the domain the way a center actually operates:

| Real tutoring-center reality | System concept |
|---|---|
| "Did the student actually show up to this class?" | Attendance is the **truth**; quota is accounting |
| Students pay after lessons, session by session | Postpaid quota rows |
| The next payment covers the **oldest unpaid sessions** first | FIFO quota coverage |
| "Take a make-up in the other group, same material" | Make-up consumes the student's **own** quota; `IsMakeUp` is derived, never stored |
| A student who hasn't paid yet still attends | Attendance is **never** blocked over money — it's simply marked unpaid |
| New month, fresh page in the notebook | New month = a **new quota row**, history is never mutated |

The goal was to combine **first-hand knowledge of the private-tutoring economy** with modern software engineering practices — the same approach that produced [HotelManager](https://github.com/A7medMubarak/HotelManager) and [PaintShop](https://github.com/A7medMubarak/paint-shop).

---

# Live Demo

| Service | Link |
|---------|------|
| Frontend | https://tutoring-hub-psi.vercel.app |
| Backend API | https://tutoringhub.runasp.net |

**Demo login — Teacher:** `admin` / `admin123` · **Student:** `01055557777` / PIN `2468`

> Hosted on a free-tier plan — the API may take a few seconds to wake up after being idle. The demo database seeds 1 center, 2 class groups, 5 students, sessions, quota rows and a full attendance history automatically (idempotent).

---

# Key Features

## Class Groups & Sessions

- Physical centers with reusable class groups (day of week, start time, weekly frequency)
- Sessions are real dates — past sessions drive attendance; future sessions drive planning
- UNIQUE `(ClassGroupId, Date)` enforced at the database level

## Attendance & Make-up

- Tick / un-tick a student per session date in one tap
- **Undo is symmetric**: un-ticking reverses quota consumption exactly (no drift)
- A student attending a class they're not enrolled in is a **make-up** — it consumes their **own** class quota, and `IsMakeUp` is derived on read, never stored

## Postpaid Billing (FIFO Quotas)

- `QuotaRow`  = total sessions, remaining sessions, price, period start/end, paid-at
- New payment covers the **oldest unpaid sessions first**
- Rows are **append-only**: a new month is a new row; history is never deleted or rewritten
- Attendance is never blocked over money — unpaid sessions are marked unpaid, the teacher sees the true picture

## Enrollment & Student Portal

- Students log in with **phone number + 4-digit PIN** (no password, no email)
- Students see their classes, attendance history, live quota balance, and quiz results

## AI-Assisted Quiz Generation

- Google Gemini drafts a quiz from the teacher's design (subject, class, topics, count, type)
- **Two-step human-in-the-loop**: the teacher reviews and approves the draft before anything is saved — nothing is written to the database silently
- API key lives server-side only; generation retries on transient provider errors (429/503)

## User Experience

- English **and** Arabic (i18next) with automatic RTL flip
- Dark mode
- Mobile-first Tailwind UI
- Separate teacher and student layouts

---

## Business Rules

- Attendance is the truth; quota is accounting — never block attendance for money
- Postpaid auto-FIFO: a new quota row covers the oldest unpaid sessions first
- Quota rows are append-only; new month = new row; never delete or reset history
- Make-up consumes the student's own class quota; `IsMakeUp` derived, never stored
- Undo (un-tick attendance) reverses consumption symmetrically
- Sessions unique `(ClassGroupId, Date)`; attendance unique `(ClassSessionId, StudentId)` — DB-level race safety
- One teacher per center; every query is tenant-scoped by the teacher from JWT claims

---

## Security

- JWT access tokens **plus rotating refresh tokens** (SHA-256 hashed at rest; replay → 401 "already been used")
- BCrypt password hashing; student PINs validated by `^\d{4}$`
- 15 FluentValidation validators, auto-applied to every request
- Global exception handler mapping exceptions to consistent HTTP status codes
- Per-IP rate limiting on auth (5/min) and AI endpoints
- No secrets in git: `Jwt:Key`, connection string, Gemini key, and CORS origins come from environment variables only

---

# Technology Stack

## Backend

- ASP.NET Core 8
- Entity Framework Core 8 (code-first, 4 migrations, auto-applied on startup)
- SQL Server
- FluentValidation 12 + SharpGrip AutoValidation
- JWT Bearer + rotating refresh tokens + BCrypt.Net
- Serilog (file + console)
- xUnit + FluentAssertions + EF Core InMemory (141 tests)

## Frontend

- React 19
- JavaScript (JSX)
- Vite 8
- Tailwind CSS 4
- Axios
- React Router 7
- i18next (AR/EN + RTL)

## DevOps

- GitHub Actions (test gate → publish → WebDeploy sync)
- MonsterASP.NET (API + SQL Server)
- Vercel (SPA)

---

# Architecture

The solution follows **Clean Architecture** to separate business rules from infrastructure and presentation concerns.

```
Frontend (React 19)
        │  HTTP / REST
        ▼
ASP.NET Core API (thin controllers, JWT, rate limiting)
        │
        ▼
Application (services, DTOs, 15 validators)
        │
        ▼
Domain (13 entities, enums — zero dependencies)
        ▲
        │
Infrastructure (EF Core, SQL Server, JWT, Gemini client)
```

For the complete architecture explanation:

➡ **docs/ENGINEERING.md**

---

# Engineering Highlights

✔ Clean Architecture with enforced dependency direction

✔ 141 automated tests (services, validators, DI smoke test)

✔ CI/CD pipeline with test gate before deploy

✔ Production deployment (MonsterASP.NET + Vercel + SQL Server)

✔ Attendance-first domain model — quota is accounting, not truth

✔ Postpaid FIFO billing with append-only quota history

✔ Derived make-up detection (never stored)

✔ Rotating refresh tokens with SHA-256 storage + replay detection

✔ JWT-claim tenancy (one teacher per center, no ID from URL/body)

✔ FluentValidation auto-applied on all 15 request shapes

✔ Human-in-the-loop AI quiz drafts (Gemini, server-side key)

✔ Business-driven design from an Education degree + live teacher/student workflow

---

# Testing

Three layers, all green:

- **Service tests** — attendance tick/untick (FIFO consumption + symmetric undo), make-up coverage, quota pay, enrollments, auth + refresh-token rotation, quizzes
- **Validator tests** — all 15 request shapes incl. PIN and login rules
- **DI smoke test** — fails if any service registration is ever missed

Result

✅ 141 passing tests

---

# API Endpoints

### Auth & Current User

| Method | Route | Description |
|---|---|---|
| POST | `/api/auth/teachers/login` | Teacher login → access + refresh token |
| POST | `/api/auth/students/login` | Student login (phone + PIN) |
| POST | `/api/auth/teachers/register` | Create teacher account |
| POST | `/api/auth/refresh` | Rotate refresh token |
| POST | `/api/auth/logout` | Revoke refresh token |
| GET | `/api/auth/me` | Current teacher profile |
| GET | `/api/me/classes` · `/api/me/attendance` · `/api/me/quotas` | Student portal data |
| GET | `/api/me/quizzes` · `/api/me/quizzes/{id}` | Student quiz list / detail |
| POST | `/api/me/quizzes/{id}/attempts` | Submit a quiz attempt |

### Centers, Classes & Sessions (Teacher-scoped)

| Method | Route | Description |
|---|---|---|
| GET/POST | `/api/centers` | List / create center |
| GET/POST | `/api/classes` | List / create class group |
| GET | `/api/classes/{id}/sessions` | Session history for a class group |
| POST/DELETE | `/api/classes/{id}/enrollments` · `/api/classes/{id}/enrollments/{studentId}` | Enroll / unenroll |

### Attendance

| Method | Route | Description |
|---|---|---|
| GET | `/api/classes/{id}/attendance?date=` | Roster for a session date (present / make-up / unpaid) |
| POST | `/api/classes/{id}/attendance` | Tick present (`{ studentId, date }`) — auto FIFO consume + derived make-up |
| DELETE | `/api/classes/{id}/attendance/{studentId}?date=` | Untick — reverses consumption symmetrically |

### Students & Quotas

| Method | Route | Description |
|---|---|---|
| GET/POST | `/api/students` | List / create student (with 4-digit PIN) |
| GET | `/api/students/{id}` | Student detail |
| GET/POST | `/api/students/{id}/quotas` | Quota rows (append-only) |
| POST | `/api/students/{id}/quotas/{quotaId}/pay` | Mark quota paid |

### Quizzes & AI

| Method | Route | Description |
|---|---|---|
| GET/POST | `/api/quizzes` | List / create quiz draft |
| GET | `/api/quizzes/{id}` · `/api/quizzes/{id}/results` | Detail / results |
| POST | `/api/quizzes/{id}/publish` · `/api/quizzes/{id}/unpublish/{classGroupId}` | Publish / unpublish |
| DELETE | `/api/quizzes/{id}` | Delete quiz |
| POST | `/api/ai/generate` | Gemini draft from quiz design (reviewed before save) |

---

# Quick Start

```bash
git clone https://github.com/A7medMubarak/TutoringHub
cd TutoringHub
```

**Backend** (needs SQL Server — migrations apply automatically on startup):

```bash
cd src/TutoringHub.API
dotnet run
```

API → `http://localhost:5293` (Swagger in Development)

**Frontend:**

```bash
cd frontend
npm install
npm run dev
```

App → `http://localhost:5174` (proxied to the API)

**Login:** Teacher `admin` / `admin123` · Student `01055557777` / `2468` · **Tests:** `dotnet test`

---

# Project Structure

```
TutoringHub/
├── src/
│   ├── TutoringHub.Domain/          # 13 entities, 6 enums, zero external deps
│   ├── TutoringHub.Application/     # 13 services, 42 DTOs, 15 validators
│   ├── TutoringHub.Infrastructure/  # EF Core configs, 4 migrations, JWT, Gemini
│   └── TutoringHub.API/             # 13 controllers, middleware, extensions
├── frontend/                        # React 19 SPA (EN/AR + RTL, dark mode)
├── tests/
│   └── TutoringHub.Application.Tests/  # 141 xUnit tests
├── docs/
│   ├── ENGINEERING.md               # architecture case study
│   └── screenshots/                 # preview images
├── .github/workflows/               # CI: test → publish → WebDeploy
├── AGENTS.md                        # contributor conventions
└── TutoringHub.sln
```

---

# Engineering Documentation

A complete case study covering the technical decisions: the private-tutoring economy → domain model, attendance-as-truth design, FIFO billing, derived make-up, JWT tenancy, refresh-token rotation, the request lifecycle, AI human-in-the-loop, testing strategy, deployment, and lessons learned.

📖 **Read:** [docs/ENGINEERING.md](docs/ENGINEERING.md)

---

# Deployment

| Layer | Platform |
|---|---|
| API + SQL Server | MonsterASP.NET (WebDeploy via GitHub Actions) |
| Frontend SPA | Vercel (auto-deploy on push, `VITE_API_URL` env) |
| Database | `Migrate()` + idempotent seed on startup (`SeedDemo` flag) |
| Secrets | Environment variables only — nothing sensitive in git |

---

# Roadmap

## Completed

- Clean Architecture (.NET 8 + React 19)
- JWT auth + rotating refresh tokens with replay detection
- Attendance-first model: tick/untick, FIFO quotas, derived make-up
- Teacher + student portals, PIN login
- AI quiz drafts with human-in-the-loop approval
- 141 automated tests + DI smoke test
- CI/CD with test gate
- Production deployment (MonsterASP.NET + Vercel)
- Arabic / English with RTL, dark mode

## Planned

- TypeScript migration of the frontend
- Integration tests against a real SQL Server database
- HTTP-only secure cookies for tokens
- Health-check endpoints
- Frontend component tests

---

# About Me

Hi, I'm **Ahmed**.

I hold a **Faculty of Education degree in French Literature from Al-Arish University** and spent **2 years in hotel operations** and **5 years in retail paint sales** before transitioning into software engineering. This is my **third production system** — each one modeled on a business I worked inside first: [HotelManager](https://github.com/A7medMubarak/HotelManager) (hospitality), [PaintShop](https://github.com/A7medMubarak/paint-shop) (retail), and now TutoringHub (private tutoring). I build the system around the operator's real day, not around a framework's example.

I'm currently seeking a Software Engineer opportunity where I can contribute, keep learning, and grow alongside experienced engineers. If you have feedback or want to discuss the project, I'd be happy to connect.

---

⭐ If you found this project interesting, consider giving it a star.