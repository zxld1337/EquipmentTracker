# 002: Supervisor approval for calibrations, with audit log

**Status:** draft (set to ready after 001 is merged)

## Context
Today any calibration takes effect immediately: a `Fail` or `Conditional` result changes `NextDueDate` with no second pair of eyes, and nothing records who changed what.

## Goal
`Fail` and `Conditional` calibrations need a supervisor's approval before they change the equipment's due date, and every approval decision is written to an audit log.

## Non-goals
- No authentication system: the supervisor is identified by a name in the request (real auth is a later spec).
- No UI change.
- No EF migration written by the agent: the human generates it after review (`dotnet ef migrations add CalibrationApproval`).

## Acceptance criteria
- [ ] AC1: `CalibrationRecord` gets `ApprovalStatus` (`Approved`, `Pending`, `Rejected`), `ReviewedBy`, `ReviewedAt`, `ReviewComment`.
- [ ] AC2: A `Pass` calibration is `Approved` on creation and recalculates `NextDueDate` as today.
- [ ] AC3: A `Fail` or `Conditional` calibration is `Pending` and does **not** change `NextDueDate`.
- [ ] AC4: `POST /api/equipment/{id}/calibrations/{calibrationId}/approve` with `{ reviewedBy, comment? }` sets `Approved`, then recalculates `NextDueDate` from approved calibrations only.
- [ ] AC5: `.../reject` with `{ reviewedBy, comment }` (comment required) sets `Rejected`; `NextDueDate` unchanged.
- [ ] AC6: Approving or rejecting a calibration that is not `Pending` returns `ErrorType.Conflict` (409).
- [ ] AC7: `reviewedBy` must differ from the calibration's `PerformedBy` (four-eyes rule), otherwise `ErrorType.Validation` (400).
- [ ] AC8: Every create, approve and reject writes an `AuditEntry` (`EntityType`, `EntityId`, `Action`, `PerformedBy`, `At` (UTC from `TimeProvider`), `OldValue`, `NewValue` as JSON). Audit entries are append-only: no update or delete method exists.
- [ ] AC9: `GET /api/equipment/{id}/audit` returns the entries for that equipment and its calibrations, newest first.

## Constraints
- May change: `Domain/**`, `Application/**`, `Infrastructure/Persistence/**` (not `Migrations/`), `Api/Controllers/**`, `Tests/UnitTests/**`.
- New dependencies: none.
- Approval state transitions live in the domain (`CalibrationRecord.Approve(...)` / `Reject(...)`), not in the service.

## Test plan
- Domain tests for every transition, including the invalid ones (AC6, AC7).
- Service tests with the fake repository and fake `TimeProvider`: due date effect (AC2-AC5) and audit entries (AC8).

## Review focus
- Can a client skip the approval by sending a status in the create request? (It must not.)
- Is the due date recalculated from approved calibrations only, in every path?
- Audit entry written in the same `SaveChangesAsync` as the state change (no lost audit on failure).
