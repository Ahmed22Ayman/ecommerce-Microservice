import { Component } from '@angular/core';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [ButtonModule],
  template: `
    <div style="padding:1rem">
      <h2>Your Cart</h2>
      <p>Cart items will be displayed here.</p>
      <button pButton type="button" label="Proceed to Checkout" icon="pi pi-credit-card"></button>
    </div>
  `
})
export class CartComponent {}
