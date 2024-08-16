import { TestBed } from '@angular/core/testing';

import { VoucherProviderService } from './voucher-provider.service';

describe('VoucherProviderService', () => {
  let service: VoucherProviderService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(VoucherProviderService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
