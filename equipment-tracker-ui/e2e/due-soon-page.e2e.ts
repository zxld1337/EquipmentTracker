import { test, expect, Page, Route } from '@playwright/test';

// Shape matches Equipment in src/app/models/equipment.model.ts (EquipmentListItemDto).
const overdue = {
  id: 1, name: 'Caliper A', barcode: 'BC-001', location: 'Lab 1', status: 'Active',
  calibrationIntervalMonths: 12, nextDueDate: '2026-11-01T00:00:00', isDue: true,
};
const soon = {
  id: 2, name: 'Multimeter B', barcode: 'BC-002', location: 'Lab 2', status: 'Active',
  calibrationIntervalMonths: 6, nextDueDate: '2026-11-25T00:00:00', isDue: false,
};
const later = {
  id: 3, name: 'Scale C', barcode: 'BC-003', location: 'Store', status: 'Active',
  calibrationIntervalMonths: 24, nextDueDate: '2027-01-20T00:00:00', isDue: false,
};

const cors = { 'access-control-allow-origin': '*' };
const json = (route: Route, body: unknown, status = 200) =>
  route.fulfill({ status, headers: cors, contentType: status >= 400 ? 'application/problem+json' : 'application/json', body: JSON.stringify(body) });

const isDue = (u: URL) => u.pathname.endsWith('/api/equipment/due');
const isList = (u: URL) => u.pathname.endsWith('/api/equipment');

/** Stubs the due endpoint; handler gets the days query value. Returns the list of days requested. */
async function stubDue(page: Page, handler: (days: string | null, route: Route) => Promise<void> | void) {
  const requested: (string | null)[] = [];
  await page.route(isDue, async route => {
    const days = new URL(route.request().url()).searchParams.get('days');
    requested.push(days);
    await handler(days, route);
  });
  return requested;
}

const daysInput = (page: Page) => page.getByRole('spinbutton', { name: 'Days' });
const showButton = (page: Page) => page.getByRole('button', { name: 'Show' });
const bodyRows = (page: Page) => page.getByRole('row').filter({ has: page.getByRole('cell') });

test.beforeEach(async ({ page }) => {
  await page.route(isList, route => json(route, [overdue, soon, later]));
});

test('AC1: "Due soon" nav link opens /equipment/due and loads the 30-day list', async ({ page }) => {
  await stubDue(page, (_d, route) => json(route, [overdue, soon]));
  await page.goto('/equipment');
  await page.getByRole('link', { name: 'Due soon' }).click();
  await expect(page).toHaveURL(/\/equipment\/due$/);
  await expect(page.getByRole('cell', { name: 'Caliper A' })).toBeVisible();
});

test('AC2: page load requests GET /api/equipment/due?days=30', async ({ page }) => {
  const requestPromise = page.waitForRequest(r => isDue(new URL(r.url())));
  await stubDue(page, (_d, route) => json(route, []));
  await page.goto('/equipment/due');
  const request = await requestPromise;
  expect(request.method()).toBe('GET');
  expect(new URL(request.url()).searchParams.get('days')).toBe('30');
});

test('AC3: table shows Name, Barcode, Location, Next due (yyyy-MM-dd), Status in API order', async ({ page }) => {
  await stubDue(page, (_d, route) => json(route, [overdue, soon]));
  await page.goto('/equipment/due');
  await expect(page.getByRole('columnheader')).toHaveText(['Name', 'Barcode', 'Location', 'Next due', 'Status']);
  await expect(bodyRows(page)).toHaveCount(2);
  await expect(bodyRows(page).nth(0).getByRole('cell')).toHaveText(['Caliper A', 'BC-001', 'Lab 1', '2026-11-01', 'Overdue']);
  await expect(bodyRows(page).nth(1).getByRole('cell')).toHaveText(['Multimeter B', 'BC-002', 'Lab 2', '2026-11-25', 'Due soon']);
});

test('AC4: isDue=true shows <mark>Overdue</mark>, otherwise "Due soon"', async ({ page }) => {
  await stubDue(page, (_d, route) => json(route, [overdue, soon]));
  await page.goto('/equipment/due');
  const overdueRow = page.getByRole('row', { name: /Caliper A/ });
  const soonRow = page.getByRole('row', { name: /Multimeter B/ });
  await expect(overdueRow.getByRole('mark')).toHaveText('Overdue');
  await expect(soonRow.getByRole('cell').last()).toHaveText('Due soon');
  await expect(soonRow.getByRole('mark')).toHaveCount(0);
});

test('AC5: entering 90 and pressing Show (or Enter) shows the days=90 result', async ({ page }) => {
  const requested = await stubDue(page, (d, route) => json(route, d === '90' ? [overdue, soon, later] : [overdue]));
  await page.goto('/equipment/due');
  await expect(bodyRows(page)).toHaveCount(1);

  await daysInput(page).fill('90');
  await showButton(page).click();
  await expect(page.getByRole('cell', { name: 'Scale C' })).toBeVisible();
  await expect(bodyRows(page)).toHaveCount(3);

  await daysInput(page).fill('30');
  await showButton(page).click();
  await expect(bodyRows(page)).toHaveCount(1);

  await daysInput(page).fill('90');
  await daysInput(page).press('Enter');
  await expect(bodyRows(page)).toHaveCount(3);
  expect(requested).toEqual(['30', '90', '30', '90']);
});

test('AC6: 1 and 365 are accepted and sent', async ({ page }) => {
  const requested = await stubDue(page, (d, route) => json(route, d === '30' ? [overdue] : []));
  await page.goto('/equipment/due');
  await expect(bodyRows(page)).toHaveCount(1);

  for (const n of ['1', '365']) {
    await daysInput(page).fill(n);
    await showButton(page).click();
    await expect(page.getByText(`No equipment due in the next ${n} days.`)).toBeVisible();
  }
  expect(requested).toEqual(['30', '1', '365']);
  await expect(page.getByText('Enter a whole number between 1 and 365.')).toHaveCount(0);
});

test('AC7: 0, 366, negative, decimal or empty sends no request and shows inline message', async ({ page }) => {
  const requested = await stubDue(page, (_d, route) => json(route, [overdue]));
  await page.goto('/equipment/due');
  await expect(bodyRows(page)).toHaveCount(1);

  for (const value of ['0', '366', '-5', '2.5', '']) {
    await daysInput(page).fill(value);
    await showButton(page).click();
    await expect(page.getByText('Enter a whole number between 1 and 365.'), `value "${value}"`).toBeVisible();
    // Reset so the next invalid value has to show the message again.
    await daysInput(page).fill('30');
    await showButton(page).click();
    await expect(page.getByText('Enter a whole number between 1 and 365.')).toHaveCount(0);
    await expect(bodyRows(page)).toHaveCount(1);
  }
  // Only the initial load and the five valid resets reached the API.
  expect(requested).toEqual(['30', '30', '30', '30', '30', '30']);
});

test('AC8: 400 ProblemDetails shows its detail and the page stays usable', async ({ page }) => {
  await stubDue(page, (d, route) =>
    d === '30'
      ? json(route, { type: 'https://tools.ietf.org/html/rfc9110#section-15.5.1', title: 'Bad Request', status: 400, detail: 'Days must be between 1 and 365.' }, 400)
      : json(route, [soon]));
  await page.goto('/equipment/due');
  await expect(page.getByText('Days must be between 1 and 365.')).toBeVisible();

  await daysInput(page).fill('60');
  await showButton(page).click();
  await expect(page.getByRole('cell', { name: 'Multimeter B' })).toBeVisible();
  await expect(page.getByText('Days must be between 1 and 365.')).toHaveCount(0);
});

test('AC9: empty list shows "No equipment due in the next N days." with current N', async ({ page }) => {
  await stubDue(page, (_d, route) => json(route, []));
  await page.goto('/equipment/due');
  await expect(page.getByText('No equipment due in the next 30 days.')).toBeVisible();

  await daysInput(page).fill('7');
  await showButton(page).click();
  await expect(page.getByText('No equipment due in the next 7 days.')).toBeVisible();
});

test('AC10: shows "Loading..." while pending, and a generic message on other HTTP errors', async ({ page }) => {
  let release!: () => void;
  const gate = new Promise<void>(r => (release = r));
  await stubDue(page, async (d, route) => {
    if (d === '30') {
      await gate;
      return json(route, [overdue]);
    }
    return json(route, { title: 'Internal Server Error', status: 500 }, 500);
  });
  await page.goto('/equipment/due');
  await expect(page.getByText('Loading...')).toBeVisible();
  release();
  await expect(page.getByRole('cell', { name: 'Caliper A' })).toBeVisible();
  await expect(page.getByText('Loading...')).toHaveCount(0);

  await daysInput(page).fill('45');
  await showButton(page).click();
  await expect(page.getByText('Could not load due equipment.')).toBeVisible();
});

test('AC11: existing Equipment list still renders with its actions', async ({ page }) => {
  await page.goto('/equipment');
  await expect(bodyRows(page)).toHaveCount(3);
  const row = page.getByRole('row', { name: /Caliper A/ });
  await expect(row.getByRole('cell').first()).toHaveText('Caliper A');
  await expect(row.getByRole('mark')).toHaveText('Due');
  await expect(row.getByRole('button')).toHaveText(['Edit', 'Delete']);
  await expect(page.getByRole('button', { name: 'New equipment' })).toBeVisible();
});
