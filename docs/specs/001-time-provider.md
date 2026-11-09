# 001: Inject TimeProvider, single source of truth for IsDue

**Status:** ready

## Context
- `EquipmentService` and `CalibrationService` call `DateTime.Today` / `DateTime.UtcNow` directly, so date logic cannot be tested deterministically (README roadmap item).
- `EquipmentService.MapToListItem` (used by `CreateAsync` and `UpdateAsync`) re-implements `IsDue` as `NextDueDate <= DateTime.Today` and ignores `Status`. A **Retired** device with a past due date is therefore returned with `isDue: true` from `POST`/`PUT`, while `GET` (which uses `Equipment.IsDue`) returns `false`. The README states retired devices are never flagged.

## Goal
Services get the current time from an injected `TimeProvider`, and every list-item DTO is built through `EquipmentMappingExtensions.ToListItemDto`.

## Non-goals
- `Infrastructure/Persistence/DbSeeder.cs` keeps using `DateTime.Today`.
- No change to the API contract, DTO shapes, or the database schema.
- No frontend change.

## Acceptance criteria
- [ ] AC1: `EquipmentService` and `CalibrationService` take a `TimeProvider` constructor parameter; `Application` contains no `DateTime.Now`, `DateTime.Today` or `DateTime.UtcNow`.
- [ ] AC2: "Today" is `timeProvider.GetLocalNow().Date`; `DeletedAt` is `timeProvider.GetUtcNow().UtcDateTime`.
- [ ] AC3: `TimeProvider.System` is registered as a singleton in `Application/DependencyInjection.cs`.
- [ ] AC4: `MapToListItem` is removed; `CreateAsync` and `UpdateAsync` use `ToListItemDto(today)`.
- [ ] AC5: Updating a Retired device whose `NextDueDate` is in the past returns `IsDue == false`.
- [ ] AC6: Adding a calibration dated after the fake "today" returns `ErrorType.Validation`; dated exactly "today" succeeds.
- [ ] AC7: A `Fail` calibration sets `NextDueDate` to the fake "today".
- [ ] AC8: `DeleteAsync` sets `DeletedAt` to the fake UTC now.

## Constraints
- May change: `Application/Services/*`, `Application/DependencyInjection.cs`, `Tests/UnitTests/**`.
- New dependencies: none. Write a small `FakeTimeProvider : TimeProvider` in `Tests/UnitTests/Application/Fakes` (override `GetUtcNow()` and `LocalTimeZone`, use UTC as local zone).

## Test plan
- Update existing service tests to pass the fake time provider.
- New tests for AC5-AC8 in `EquipmentServiceTests` and a new `CalibrationServiceTests`.

## Review focus
- Local vs UTC: is "today" computed the same way everywhere?
- AC5 test actually fails on the old `MapToListItem` code.
- `TimeProvider` lifetime (singleton) vs scoped services.
