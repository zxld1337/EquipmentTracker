Status: Approved 2026-11-10

# Due soon page

## User story
As the person who tracks calibrations, I want a page that lists the equipment due within a number of days I choose, so that I can plan the next calibrations without scanning the whole list.

## Acceptance criteria
1. Given the app is open, when I click "Due soon" in the top nav, then I land on `/equipment/due` and the list for the next 30 days loads straight away.
2. Given the page loads, when the request is sent, then it calls `GET /api/equipment/due?days=30` through the equipment service. Components make no HTTP calls of their own.
3. Given equipment is returned, when the table renders, then it shows Name, Barcode, Location, Next due (yyyy-MM-dd) and Status, in the order the API returns (soonest first, overdue at the top).
4. Given an item has `isDue = true`, when it renders, then its Status shows a `<mark>Overdue</mark>`. Otherwise Status shows "Due soon".
5. Given I enter 90 and press "Show" (or Enter), when the request finishes, then the table shows the result of `days=90`.
6. Given I enter 1 or 365, when I press "Show", then the request is sent (the limits are inclusive).
7. Given I enter 0, 366, a negative number, a decimal or nothing, when I press "Show", then no request is sent and an inline message reads "Enter a whole number between 1 and 365."
8. Given the API returns a 400 ProblemDetails anyway, when it arrives, then its `detail` text is shown as an error and the page stays usable.
9. Given the API returns an empty list, when it renders, then the page shows "No equipment due in the next N days." with the current N.
10. Given the request is still running, when the page renders, then it shows "Loading...". Given any other HTTP error, it shows "Could not load due equipment."
11. Given the existing Equipment list, when this feature ships, then that list and its tests work exactly as before.

## Decisions
- Default is 30 days, sent explicitly as `?days=30`: it matches the API default and the owner can see it in the request.
- Validate 1–365 whole numbers in the UI as well as in the API: this saves a pointless 400, and the API stays the real gate.
- The page is read-only, with no Edit/Delete/New buttons: it is a planning view, and editing stays on the Equipment page.
- The days input is a number field with min=1, max=365 and step=1, plus a "Show" button in one form: one explicit trigger per request, no debounce needed.
- The days value is not kept in the URL or remembered between visits: nobody asked for it, so it can come later.
- The response reuses the existing `Equipment` UI model: `EquipmentListItemDto` has exactly the same fields.
- The route goes before the `**` wildcard, and the nav link sits next to "Equipment": consistent with the existing routing and nav.
- "Overdue" is driven by the backend `isDue` flag, with no date maths in the UI: business rules stay in the backend.
- Tests: a service spec checks the URL and query param, and a component spec covers the default load, a valid change, invalid input sending no request, and the empty state.

## Questions for the owner
None.

## Out of scope
- No backend changes: the endpoint, its validation and its sorting already exist and are tested.
- No preset buttons (7/30/90), no remembered choice, no days in the URL.
- No retired-equipment toggle, no filtering by location, no export or notifications.
- No changes to the existing Equipment list page.

## Notes
- The backend uses `DateTime.Today`, so "today" means the server's local date, not the browser's.
