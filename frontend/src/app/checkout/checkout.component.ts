import { Component } from '@angular/core';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [ButtonModule],
  template: `
    <div style="padding:1rem">
      <h2>Checkout</h2>
      <p>Checkout flow will be implemented here.</p>
      <button pButton type="button" label="Pay Now" icon="pi pi-credit-card"></button>
    </div>
  `
})
export class CheckoutComponent {}
