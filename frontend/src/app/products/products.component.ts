import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService, Product } from '../services/api.service';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './products.component.html',
  styleUrls: ['./products.component.scss']
})
export class ProductsComponent implements OnInit {
  products: Product[] = [];
  loading = false;
  error: string | null = null;

  constructor(private apiService: ApiService) {}

  ngOnInit() {
    this.loadProducts();
  }

  loadProducts() {
    this.loading = true;
    this.error = null;
    
    this.apiService.getProducts().subscribe({
      next: (products) => {
        this.products = products;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading products:', error);
        this.error = 'Failed to load products. Using mock data.';
        this.loading = false;
        // Fallback to mock data
        this.loadMockProducts();
      }
    });
  }

  private loadMockProducts() {
    this.products = [
      {
        id: 1,
        name: 'Wireless Headphones',
        description: 'High-quality wireless headphones with noise cancellation',
        price: 199.99,
        category: 'Electronics',
        stock: 15
      },
      {
        id: 2,
        name: 'Smart Watch',
        description: 'Feature-rich smartwatch with health monitoring',
        price: 299.99,
        category: 'Electronics',
        stock: 8
      },
      {
        id: 3,
        name: 'Running Shoes',
        description: 'Comfortable running shoes for daily exercise',
        price: 89.99,
        category: 'Sports',
        stock: 0
      },
      {
        id: 4,
        name: 'Coffee Maker',
        description: 'Automatic coffee maker with programmable settings',
        price: 149.99,
        category: 'Home',
        stock: 12
      }
    ];
  }

  addToCart(product: Product) {
    this.apiService.addToCart(product.id, 1).subscribe({
      next: () => {
        console.log('Added to cart:', product.name);
        // TODO: Show success message
      },
      error: (error) => {
        console.error('Error adding to cart:', error);
        // TODO: Show error message
      }
    });
  }

  addToWishlist(product: Product) {
    this.apiService.addToWishlist(product.id).subscribe({
      next: () => {
        console.log('Added to wishlist:', product.name);
        // TODO: Show success message
      },
      error: (error) => {
        console.error('Error adding to wishlist:', error);
        // TODO: Show error message
      }
    });
  }
}
