# TutoringHub (Dars Khosousy)

Arabic-branded tutoring-center SaaS: teachers run class groups in centers, track attendance, quotas,
and attendance-based payments (postpaid, FIFO coverage). .NET 8 solution + React (Vite) frontend (later iterations).

## Commands

```powershell
# Build (solution includes Domain, Application, Infrastructure, API, Application.Tests)
dotnet build TutoringHub.sln

# Run tests
dotnet test tests/TutoringHub.Application.Tests/TutoringHub.Application.Tests.csproj

# Run only one class (faster loop)
dotnet test tests/TutoringHub.Application.Tests/TutoringHub.Application.Tests.csproj `
  --filter "FullyQualifiedName~AuthServiceTests"

# EF migrations (design-time host is API)
dotnet ef migrations add <Name> --project src/TutoringHub.Infrastructure --startup-project src/TutoringHub.API
dotnet ef database update --project src/TutoringHub.Infrastructure --startup-project src/TutoringHub.API

# Run API locally
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ASPNETCORE_URLS = "http://localhost:5293"
dotnet run --project src/TutoringHub.API
```

Goal: solution builds with **0 warnings / 0 errors**; full test suite green before any commit.

## Architecture & Layout

Clean Architecture, one project per layer (Mirror dvld/paint-shop conventions):

- `src/TutoringHub.Domain` — entities + enums + `IApplicationDbContext` (DbSet<T> per entity, SaveChangesAsync). Has EF Core dep by necessity (DbSet).
- `src/TutoringHub.Application` — DTOs, FluentValidation validators, `*.Services.Interfaces`, service implementations, `JwtOptions`.
- `src/TutoringHub.Infrastructure` — `ApplicationDbContext` (ApplyConfigurationsFromAssembly), `Persistence/Configurations/*.cs` (one Fluent config per entity), `Security/JwtTokenService`, `Migrations/`.
- `src/TutoringHub.API` — `Program.cs`, `Extensions/ServiceCollectionExtensions.cs` (AddApplicationServices, AddJwtAuthentication, AddRateLimitingPolicy, AddCorsPolicy, AddSwaggerWithJwt), `Middleware/GlobalExceptionHandler.cs`, `Services/CurrentUserService.cs`, controllers.
- `tests/TutoringHub.Application.Tests` — service tests under `Services/`, validator tests under `Validators/`, `DependencyInjectionTests.cs`, shared `TestCommon/MockDbContext.cs`.

Controllers: thin; tenancy always via `ICurrentUserService` (JWT claims) — **never** from URL/body.
All services: interface in `Application/Services/Interfaces` + `AddScoped` in `AddApplicationServices`; the DI smoke test
`DependencyInjectionTests.AllApplicationServices_ResolveWithoutError` fails if a registration is missed.

## Domain Rules (non-negotiable)

- Attendance is the truth; quota is accounting. Never block attendance for money — mark unpaid instead.
- Make-up (attending a class not enrolled in): consumes the student's OWN class quota; `IsMakeUp` derived, never stored.
- Postpaid: auto FIFO — a new quota row covers oldest unpaid sessions first. New month = new quota row; never delete/reset history.
- Undo (un-tick attendance) reverses consumption symmetrically.
- Quota rows: append-only, `TotalSessions/RemainingSessions/Price/PaidAt/PeriodStart/PeriodEnd`.
- Unique `(ClassGroupId, Date)` for sessions; unique `(ClassSessionId, StudentId)` for attendance.
- One teacher per center; TeacherId tenancy for all teacher data.

## Conventions & Gotchas

- **Microsoft packages pinned exactly `8.0.0`**; third-party at their own versions. Ask the user before adding any package/config; don't copy config blindly.
- Secrets only in `appsettings.Development.json` (gitignored). `Jwt:Key` must exist or `AddJwtAuthentication` throws; seed admin: `admin` / `admin123`, phone `0000000000` (idempotent — checks `Teachers.AnyAsync`).
- Login endpoints rate-limited by "auth" policy (5/min per IP). CORS policy `TutoringHubCorsPolicy` (5173/5174).
- Refresh tokens: one table + `Role` column; SHA-256 hashed; rotation on refresh; replay → 401 "already been used".
- Student login = Phone + teacher-set 4-digit PIN (`^\d{4}$`); no Username on Student; passwords via BCrypt.
- FluentValidation ≥10 ships the test helpers in the main package — `FluentValidation.TestHelper` is NOT a separate NuGet package (restore NU1101). Use `TestValidate`/`ShouldHaveValidationErrorFor` + FluentAssertions.
- **InMemory provider gotcha**: `.Include()` on a required relationship emulates INNER JOIN — a dependent whose FK points at a non-existent principal is silently dropped. Always build test graphs via navigation references (`Center = center`), not FK values captured before `SaveChanges` (bools: principals get Ids only after save).
- The DI smoke test needs `services.AddLogging()` (GlobalExceptionHandler takes `ILogger<>`).
- EF command gotchas: `dotnet ef ...` prints a `HostAbortedException` stack trace — that is expected design-time behavior; "Done." means success. Migrations regenerate intentionally.
- PowerShell: `Select-Object -Last` after dotnet commands can appear hung; `Start-Process` with redirected output blocks the parent shell — run the API detached without redirects. Kill a lingering `TutoringHub.API` process before rebuilds (locks bin/). `dotnet test` output → redirect to a log file, then grep `Failed |Passed:`.
- Frontend (later): Vite + React + Tailwind, mobile-first, axios interceptor refresh-on-401; Arabic UI via i18n/RTL — no Arabic in data/API.

No code comments unless asked; no dead code; no `NotImplementedException`; secrets never in code or commits.