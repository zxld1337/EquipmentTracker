import { Routes } from '@angular/router';

export const routes: Routes = [
    { path: '', redirectTo: 'equipment', pathMatch: 'full' },
    {
        path: 'equipment',
        loadComponent: () => import('./features/equipment-list/equipment-list')
            .then(m => m.EquipmentList)
    },
    {
        path: 'equipment/new',
        loadComponent: () => import('./features/equipment-form/equipment-form')
            .then(m => m.EquipmentForm)
    },
    {
        path: 'equipment/:id/edit',
        loadComponent: () => import('./features/equipment-form/equipment-form')
            .then(m => m.EquipmentForm)                 
    },
    { 
        path: '**', 
        redirectTo: 'equipment' 
    }
];
