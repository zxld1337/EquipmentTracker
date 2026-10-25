import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';

import { EquipmentList } from './equipment-list';

describe('EquipmentList', () => {
  let component: EquipmentList;
  let fixture: ComponentFixture<EquipmentList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EquipmentList],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(EquipmentList);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
