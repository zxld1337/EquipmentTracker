# Equipment Calibration Tracker

A full-stack app for tracking measuring equipment and its calibration history: which device is where, when it was last calibrated, and which ones are due.

**Backend:** ASP.NET Core 8 Web API · Entity Framework Core 8 · SQL Server (LocalDB) · xUnit v3
**Frontend:** Angular 21 · Pico CSS

## Features

- Create, list, edit and (soft) delete equipment
- Unique barcode per device; duplicates are rejected with `409 Conflict`
- Record calibrations (performer, date, result: Pass / Fail / Conditional, notes)
- The next due date is recalculated after every calibration:
  - **Pass / Conditional:** last calibration date + the device's calibration interval
  - **Fail:** due immediately
- Overdue devices are flagged (`isDue`); retired devices are never flagged and cannot receive new calibrations
- Errors are returned as RFC 7807 Problem Details (`404`, `400`, `409`)
- Interactive API docs with Scalar (development mode)

## Architecture

The backend follows a layered, Clean Architecture–style structure. Dependencies point inwards only:

```
Api  ──►  Infrastructure  ──►  Application  ──►  Domain
```

| Project | Responsibility |
|---|---|
| `Domain` | Entities (`Equipment`, `CalibrationRecord`), enums, domain rules such as `Equipment.IsDue` |
| `Application` | Use cases (`EquipmentService`, `CalibrationService`), DTOs, `Result<T>` error handling, the `IEquipmentRepository` abstraction |
| `Infrastructure` | EF Core `DbContext`, migrations, repository implementation, seed data, soft-delete query filter |
| `Api` | Controllers, mapping `Result<T>` errors to HTTP status codes, DI and CORS setup |
| `Tests/UnitTests` | xUnit v3 tests for domain rules and application services |

Services return a `Result<T>` instead of throwing exceptions for expected failures (not found, conflict, validation), and the controller maps the error type to an HTTP status code.

## API

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/equipment` | List all equipment |
| `GET` | `/api/equipment/{id}` | Equipment details with calibration history |
| `POST` | `/api/equipment` | Create equipment |
| `PUT` | `/api/equipment/{id}` | Update equipment |
| `DELETE` | `/api/equipment/{id}` | Soft delete |
| `POST` | `/api/equipment/{id}/calibrations` | Record a calibration |
| `GET` | `/api/equipment/{id}/calibrations/{calibrationId}` | Get a single calibration |

## Getting started

### Prerequisites

- [.NET 8 runtime](https://dotnet.microsoft.com/download) (the repo is built and tested with the .NET 10 SDK)
- SQL Server LocalDB (installed with Visual Studio)
- Node.js 20+ and npm
- EF Core CLI: `dotnet tool install --global dotnet-ef`

### Backend

```bash
cd EquipmentCalibrationTracker
dotnet ef database update --project Infrastructure --startup-project Api
dotnet run --project Api
```

The API listens on `http://localhost:5015`. Sample data is seeded on first start. API docs: `http://localhost:5015/scalar`.

### Frontend

```bash
cd equipment-tracker-ui
npm install
npm start
```

Open `http://localhost:4200`.

## Tests

```bash
cd EquipmentCalibrationTracker
dotnet test
```

The unit tests use xUnit v3 with the Microsoft Testing Platform runner (enabled in `global.json`). Application services are tested against a hand-written in-memory fake of `IEquipmentRepository`, so no database is needed.

## AI-agent workflow

AI agents are used as engineering tools in the pipeline, with a human owning the spec and the merge.

```
spec (human) -> agent implements + writes tests -> build & test -> draft PR -> agent review -> human review & merge
docs/specs/     agent-implement.yml: propose       verify          open-pr     agent-review.yml   PR checklist
```

1. **Spec:** write `docs/specs/NNN-name.md` from `_template.md` with testable acceptance criteria, set `Status: ready`.
2. **Implement:** Actions → *Agent implement* → run with the spec path. The agent (Claude Code, rules in `CLAUDE.md`) writes code and tests.
3. **Verify:** the patch is built and tested in a separate job with no secrets.
4. **Draft PR:** opened with the agent's summary (which test covers which criterion) and the test result.
5. **Agent review:** a second, read-only agent comments on the PR against `CLAUDE.md` and the spec.
6. **Human review:** the PR checklist; the workflow cannot approve or merge.

Guardrails:
- Least privilege: the agent jobs have a read-only token and no shell; only trusted workflow code pushes or comments.
- Agent output never runs in a job that holds a secret; build and tests run without one.
- The agent may only change application and test code. Dependency files, `.github/`, `CLAUDE.md` and build config are rejected.
- Patches and review text are scanned for token-like strings before they leave the job; the CLI version is pinned.

Setup: add a `CLAUDE_CODE_OAUTH_TOKEN` repository secret and enable *Settings → Actions → General → Allow GitHub Actions to create and approve pull requests*.

## Roadmap

- Integration tests for the API with `WebApplicationFactory`
- Inject `TimeProvider` instead of using `DateTime.Today` directly, so date logic can be tested deterministically (spec: `docs/specs/001-time-provider.md`)
- Calibration management in the UI
