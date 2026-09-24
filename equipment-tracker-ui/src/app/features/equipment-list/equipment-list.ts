import { Component, inject } from '@angular/core';
import { BehaviorSubject, Observable, switchMap } from 'rxjs';
import { Equipment } from '../../models/equipment.model';
import { CommonModule } from '@angular/common';
import { EquipmentService } from '../../services/equipment-service'
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-equipment-list',
  imports: [CommonModule, RouterLink],
  templateUrl: './equipment-list.html',
  styleUrl: './equipment-list.css',
})

export class EquipmentList {
  private service = inject(EquipmentService);

  private refresh$ = new BehaviorSubject<void>(undefined);

  equipments$: Observable<Equipment[]> = this.refresh$.pipe(
    switchMap(() => this.service.getAll())
  );

  deleteError: string | null = null;

  onDelete(item: Equipment): void {
    if (!confirm(`Delete "${item.name}"?`)) return;

    this.deleteError = null;

    this.service.delete(item.id).subscribe({
      next: () => this.refresh$.next(),
      error: (err) => this.deleteError = err.error?.detail ?? 'delete failed'
    });
  }
}
