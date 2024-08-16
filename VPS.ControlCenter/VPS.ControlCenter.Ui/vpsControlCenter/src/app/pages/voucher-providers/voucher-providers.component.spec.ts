import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VoucherProvidersComponent } from './voucher-providers.component';

describe('VoucherProvidersComponent', () => {
  let component: VoucherProvidersComponent;
  let fixture: ComponentFixture<VoucherProvidersComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VoucherProvidersComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(VoucherProvidersComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
