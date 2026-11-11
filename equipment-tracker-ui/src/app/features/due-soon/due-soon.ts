import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { Equipment } from '../../models/equipment.model';
import { EquipmentService } from '../../services/equipment-service';

@Component({
  selector: 'app-due-soon',
  imports: [DatePipe, ReactiveFormsModule],
  templateUrl: './due-soon.html',
})
export class DueSoon {
  private service = inject(EquipmentService);
  private request?: Subscription;

  days = new FormControl<number | null>(30);
  shownDays = signal(30);
  items = signal<Equipment[] | null>(null);
  error = signal<string | null>(null);
  invalid = signal(false);

  constructor() {
    this.load(30);
  }

  onShow(): void {
    const d = this.days.value;
    const valid = d !== null && Number.isInteger(d) && d >= 1 && d <= 365;
    this.invalid.set(!valid);
    if (valid) this.load(d);
  }

  private load(days: number): void {
    this.request?.unsubscribe(); // a newer request replaces a slower older one
    this.items.set(null);
    this.error.set(null);
    this.request = this.service.getDue(days).subscribe({
      next: items => {
        this.shownDays.set(days);
        this.items.set(items);
      },
      error: (err: HttpErrorResponse) =>
        this.error.set(err.status === 400 && err.error?.detail ? err.error.detail : 'Could not load due equipment.'),
    });
  }
}
