import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

import { DueSoon } from './due-soon';
import { Equipment } from '../../models/equipment.model';

const item = (id: number, isDue: boolean): Equipment => ({
  id, name: `Item ${id}`, barcode: `B${id}`, location: 'Lab', status: 'Active',
  calibrationIntervalMonths: 12, nextDueDate: '2026-11-20T00:00:00', isDue,
});

describe('DueSoon', () => {
  let fixture: ComponentFixture<DueSoon>;
  let http: HttpTestingController;
  const text = () => (fixture.nativeElement as HTMLElement).textContent ?? '';

  const expectDue = (days: number) =>
    http.expectOne(r => r.url.endsWith('/equipment/due') && r.params.get('days') === String(days));

  const show = async (value: number | null) => {
    fixture.componentInstance.days.setValue(value);
    (fixture.nativeElement as HTMLElement).querySelector('button')!.click();
    await fixture.whenStable();
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DueSoon],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    http = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(DueSoon);
    await fixture.whenStable();
  });

  afterEach(() => http.verify());

  it('loads 30 days by default and renders rows with status', async () => {
    expect(text()).toContain('Loading...');
    expectDue(30).flush([item(1, true), item(2, false)]);
    await fixture.whenStable();

    const rows = (fixture.nativeElement as HTMLElement).querySelectorAll('tbody tr');
    expect(rows.length).toBe(2);
    expect(rows[0].querySelector('mark')?.textContent).toBe('Overdue');
    expect(rows[0].textContent).toContain('2026-11-20');
    expect(rows[1].textContent).toContain('Due soon');
  });

  it('requests the entered number of days on Show, including the limits', async () => {
    expectDue(30).flush([]);
    for (const days of [90, 1, 365]) {
      await show(days);
      expectDue(days).flush([]);
    }
  });

  it('sends no request and shows a message for invalid input', async () => {
    expectDue(30).flush([]);
    for (const days of [0, 366, -5, 2.5, null]) {
      await show(days);
      expect(text()).toContain('Enter a whole number between 1 and 365.');
    }
    http.expectNone(r => r.url.endsWith('/equipment/due'));
  });

  it('shows the empty state with the current number of days', async () => {
    expectDue(30).flush([]);
    await show(90);
    expectDue(90).flush([]);
    await fixture.whenStable();
    expect(text()).toContain('No equipment due in the next 90 days.');
  });

  it('shows the ProblemDetails detail on 400 and a generic message otherwise', async () => {
    expectDue(30).flush({ detail: 'Days must be between 1 and 365.' }, { status: 400, statusText: 'Bad Request' });
    await fixture.whenStable();
    expect(text()).toContain('Days must be between 1 and 365.');

    await show(60);
    expectDue(60).flush(null, { status: 500, statusText: 'Server Error' });
    await fixture.whenStable();
    expect(text()).toContain('Could not load due equipment.');
  });
});
