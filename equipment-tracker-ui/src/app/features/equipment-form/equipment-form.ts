import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { EquipmentService } from '../../services/equipment-service';

@Component({
  selector: 'app-equipment-form',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './equipment-form.html',
  styleUrl: './equipment-form.css',
})
export class EquipmentForm implements OnInit {
  private fb = inject(FormBuilder);
  private service = inject(EquipmentService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  
  id: number | null = null;
  serverError: string | null = null;
  saving = false;

  form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    barcode: ['', [Validators.required, Validators.maxLength(50)]],
    location: ['', [Validators.required, Validators.maxLength(100)]],
    calibrationIntervalMonths: [12, [Validators.required, Validators.min(1), Validators.max(120)]]
  });

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (!idParam) return;

    this.id = Number(idParam);

    this.service.getById(this.id).subscribe({
      next: (e) => this.form.patchValue(e),
      error: () => this.serverError = 'Could not load equipment.'
    });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving = true;
    this.serverError = null;

    const value = this.form.getRawValue();

    const request$ = this.id
      ? this.service.update(this.id, value)
      : this.service.create(value);

    request$.subscribe({
      next: () => this.router.navigate(['/equipment']),
      error: (err) => {
        this.saving = false;
        this.serverError = err.error?.detail ?? 'Save failed.';
      }
    });
  }
}
