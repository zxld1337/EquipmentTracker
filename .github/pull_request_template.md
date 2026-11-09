## Spec
<!-- docs/specs/NNN-....md, or "none" for a manual change -->

## Summary

## Review checklist (agent-written code is reviewed like a junior's PR)
- [ ] Every acceptance criterion has a test, and the test would fail without the change
- [ ] Async: `CancellationToken` passed down, no `.Result` / `.Wait()` / `async void`
- [ ] EF Core: no N+1, tracking vs `AsNoTracking` is intentional, soft-delete filter respected
- [ ] Domain rules called, not duplicated (`Equipment.IsDue`, mappers)
- [ ] Expected failures return `Result<T>`, not exceptions
- [ ] Dates: no `DateTime.Now/Today/UtcNow` in Domain/Application, local vs UTC is consistent
- [ ] No new dependency, secret, or change outside the spec's allowed files

## Security (first lens, not last)
- [ ] Input validated at the boundary (DTO validation, ranges, lengths); no trust in client-sent ids or states
- [ ] Authorization: who may call this, and is it enforced server-side
- [ ] No raw SQL / string-built queries; no sensitive data in logs, errors or Problem Details
- [ ] State changes that matter (approval, delete, result change) are audit-logged with who/when/what
- [ ] Agent-written code did not weaken an existing check, filter (soft delete) or error path
- [ ] I understand every line and could defend it without the agent
