# CLAUDE.md

## 1. Purpose
Full-stack app for tracking measuring equipment and its calibration history: where each device is,
when it was last calibrated, and which ones are due. ASP.NET Core 8 Web API + Angular 21 UI.

## 2. Layout
- `EquipmentCalibrationTracker/`: .NET solution (`EquipmentCalibrationTracker.sln`), `net8.0` via `Directory.Build.props`
  - `Domain/`: entities (`Equipment`, `CalibrationRecord`), enums. No project references.
  - `Application/`: services, DTOs, mappings, `Common/Result.cs`, `Abstractions/IEquipmentRepository.cs`
  - `Infrastructure/`: EF Core `AppDbContext` (SQL Server LocalDB), migrations, `EquipmentRepository`, `DbSeeder`
  - `Api/`: `Program.cs`, `Controllers/EquipmentController.cs`
  - `Tests/UnitTests/`: xUnit v3 tests for Domain and Application
- `equipment-tracker-ui/`: Angular 21, Pico CSS, Vitest (via `@angular/build:unit-test`)
  - `src/app/features/` (components), `services/`, `models/`, `environments/`

## 3. Commands
Backend (run in `EquipmentCalibrationTracker/`, same as `.github/workflows/backend.yml`):
```
dotnet restore
dotnet build --no-restore --configuration Release
dotnet test --no-build --verbosity normal --configuration Release
```
`global.json` selects the Microsoft.Testing.Platform runner. CI installs the .NET 8 and .NET 10 SDKs.

Frontend (run in `equipment-tracker-ui/`, same as `.github/workflows/frontend.yml`, Node 24):
```
npm ci
npm test -- --watch=false
npm run build
```

## 4. Architecture rules
Project references as they are in the .csproj files:
```
Api → Infrastructure → Application → Domain
UnitTests → Application, Domain
```
- Api has no direct reference to Application or Domain. It uses their types transitively through Infrastructure.
- Domain references nothing. Application depends on `IEquipmentRepository`, and Infrastructure implements it.
- Business logic lives in:
  - Domain, for entity-level rules (e.g. `Equipment.IsDue` in `Domain/Entities/Equipment.cs`)
  - Application services, for use-case rules: validation, barcode conflict, retired-equipment checks,
    next-due-date recalculation (`Application/Services/CalibrationService.cs`)
- Business logic must not live in controllers (`Api/`) or in repositories/DbContext (`Infrastructure/`).
  Today they only call services or do data access.

## 5. Conventions
- **Service results:** services return `Result<T>` (`Application/Common/Result.cs`) and do not throw for
  expected failures. Use `Result<T>.Success(value)` / `Result<T>.Failure(ErrorType, message)`.
  `ErrorType` is `None | NotFound | Validation | Conflict`. Example: `Application/Services/CalibrationService.cs`.
  Exception: `EquipmentService.GetAllAsync` returns a `List<>` directly.
- **Failure → HTTP:** controllers check `result.IsSuccess` and call a private `ToErrorResponse`, which maps
  NotFound→404, Validation→400, Conflict→409, anything else→500 and returns `Problem(...)` (RFC 7807).
  `Program.cs` registers `AddProblemDetails()`. Example: `Api/Controllers/EquipmentController.cs`.
- **DI:** each layer exposes an `IServiceCollection` extension method. `AddApplication()` registers services as
  scoped concrete classes (no interfaces). `AddInfrastructure(configuration)` registers `AppDbContext`
  (connection string `Default`) and `IEquipmentRepository → EquipmentRepository`, both scoped.
  `Program.cs` calls both. Example: `Application/DependencyInjection.cs`, `Infrastructure/DependencyInjection.cs`.
- **Mapping:** entity→DTO through extension methods (`ToDto()`) in `Application/Mappings/EquipmentMappingExtensions.cs`.
- **Async:** service and repository methods take `CancellationToken ct = default`, and controllers pass theirs through.

## 6. Testing rules
- Backend: `EquipmentCalibrationTracker/Tests/UnitTests/`, xUnit v3 (`xunit.v3`, `Xunit` global using).
  Folders mirror the layer under test (`Domain/`, `Application/Services/`).
- Test class: `<ClassUnderTest>Tests` (e.g. `EquipmentServiceTests`).
- Test method: `Method_Condition_ExpectedResult`
  (e.g. `CreateAsync_WhenBarcodeAlreadyExists_ReturnsConflict`, `IsDue_WhenNextDueIsNull_ReturnsFalse`).
- Repository dependencies are replaced with hand-written fakes in `Application/Fakes/`
  (`FakeEquipmentRepository`). No mocking library is referenced.
- Async tests pass `TestContext.Current.CancellationToken`.
- Frontend: `*.spec.ts` next to the source file, `describe`/`it` with Angular `TestBed`, run by Vitest.
- TODO (owner decides): are Api/Infrastructure tests (integration, EF) wanted, and where would they go?

## 7. Forbidden patterns
TODO (owner decides): none confirmed yet.
<!-- PROPOSALS ONLY, not yet agreed:
  - Throwing exceptions for expected failures instead of returning Result<T>.Failure
  - Business rules in controllers, repositories or AppDbContext
  - Api using AppDbContext or EquipmentRepository directly instead of an Application service
  - Returning entities from controllers instead of DTOs
  - Adding a project reference that points outward (e.g. Domain → Application)
  - Committing real connection strings or secrets to appsettings*.json
  - Editing an applied migration instead of adding a new one
-->

## 8. Definition of done
- Backend and frontend build with the commands in section 3.
- All backend and frontend tests pass.
- No test deleted, skipped or weakened to make a change pass.
- No secrets committed (keys, passwords, real connection strings).
