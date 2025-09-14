import { Component } from '@angular/core';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'app-wishlist',
  standalone: true,
  imports: [ButtonModule],
  template: `
    <div style="padding:1rem">
      <h2>Wishlist</h2>
      <p>Wishlist items will be displayed here.</p>
      <button pButton type="button" label="Move All to Cart" icon="pi pi-shopping-cart"></button>
    </div>
  `
})
export class WishlistComponent {}
