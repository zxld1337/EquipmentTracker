import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

import { EquipmentService } from './equipment-service';

describe('EquipmentService', () => {
  let service: EquipmentService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(EquipmentService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('getDue calls GET /equipment/due with the days query param', () => {
    const http = TestBed.inject(HttpTestingController);
    let result: unknown;
    service.getDue(30).subscribe(r => (result = r));

    const req = http.expectOne(r => r.url.endsWith('/api/equipment/due'));
    expect(req.request.method).toBe('GET');
    expect(req.request.params.get('days')).toBe('30');
    req.flush([]);
    expect(result).toEqual([]);
    http.verify();
  });
});
