import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';


import { EquipmentForm } from './equipment-form';

describe('EquipmentForm', () => {
  let component: EquipmentForm;
  let fixture: ComponentFixture<EquipmentForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EquipmentForm],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(EquipmentForm);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
