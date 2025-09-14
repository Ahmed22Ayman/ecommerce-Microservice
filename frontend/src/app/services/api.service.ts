import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';

export interface Product {
  id: number;
  name: string;
  description: string;
  price: number;
  category: string;
  stock: number;
  imageUrl?: string;
}

export interface User {
  id: number;
  username: string;
  email: string;
  role: string;
}

export interface CartItem {
  id: string;
  userId: number;
  productId: number;
  quantity: number;
  product?: Product;
}

export interface Order {
  id: number;
  userId: number;
  orderDate: string;
  status: string;
  totalAmount: number;
  items: OrderItem[];
}

export interface OrderItem {
  id: number;
  orderId: number;
  productId: number;
  quantity: number;
  price: number;
  product?: Product;
}

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private readonly baseUrl = 'http://localhost:8080/api'; // API Gateway URL
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {
    // Check for existing token on service initialization
    const token = localStorage.getItem('jwt_token');
    if (token) {
      // TODO: Validate token and get user info
    }
  }

  private getAuthHeaders(): HttpHeaders {
    const token = localStorage.getItem('jwt_token');
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': token ? `Bearer ${token}` : ''
    });
  }

  // Auth Service methods
  login(email: string, password: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/auth/login`, { email, password });
  }

  register(userData: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/auth/register`, userData);
  }

  logout(): void {
    localStorage.removeItem('jwt_token');
    this.currentUserSubject.next(null);
  }

  setCurrentUser(user: User, token: string): void {
    localStorage.setItem('jwt_token', token);
    this.currentUserSubject.next(user);
  }

  // Product Service methods
  getProducts(): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.baseUrl}/products`);
  }

  getProduct(id: number): Observable<Product> {
    return this.http.get<Product>(`${this.baseUrl}/products/${id}`);
  }

  searchProducts(query: string): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.baseUrl}/products/search?q=${query}`);
  }

  getProductsByCategory(category: string): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.baseUrl}/products/category/${category}`);
  }

  // Cart Service methods
  getCart(): Observable<CartItem[]> {
    return this.http.get<CartItem[]>(`${this.baseUrl}/cart`, { 
      headers: this.getAuthHeaders() 
    });
  }

  addToCart(productId: number, quantity: number): Observable<any> {
    return this.http.post(`${this.baseUrl}/cart`, 
      { productId, quantity }, 
      { headers: this.getAuthHeaders() }
    );
  }

  updateCartItem(itemId: string, quantity: number): Observable<any> {
    return this.http.put(`${this.baseUrl}/cart/${itemId}`, 
      { quantity }, 
      { headers: this.getAuthHeaders() }
    );
  }

  removeFromCart(itemId: string): Observable<any> {
    return this.http.delete(`${this.baseUrl}/cart/${itemId}`, { 
      headers: this.getAuthHeaders() 
    });
  }

  clearCart(): Observable<any> {
    return this.http.delete(`${this.baseUrl}/cart`, { 
      headers: this.getAuthHeaders() 
    });
  }

  // Wishlist methods (assuming part of cart service)
  getWishlist(): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.baseUrl}/wishlist`, { 
      headers: this.getAuthHeaders() 
    });
  }

  addToWishlist(productId: number): Observable<any> {
    return this.http.post(`${this.baseUrl}/wishlist`, 
      { productId }, 
      { headers: this.getAuthHeaders() }
    );
  }

  removeFromWishlist(productId: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/wishlist/${productId}`, { 
      headers: this.getAuthHeaders() 
    });
  }

  // Order Service methods
  getOrders(): Observable<Order[]> {
    return this.http.get<Order[]>(`${this.baseUrl}/orders`, { 
      headers: this.getAuthHeaders() 
    });
  }

  getOrder(id: number): Observable<Order> {
    return this.http.get<Order>(`${this.baseUrl}/orders/${id}`, { 
      headers: this.getAuthHeaders() 
    });
  }

  createOrder(orderData: any): Observable<Order> {
    return this.http.post<Order>(`${this.baseUrl}/orders`, 
      orderData, 
      { headers: this.getAuthHeaders() }
    );
  }

  // Payment Service methods
  processPayment(paymentData: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/payments`, 
      paymentData, 
      { headers: this.getAuthHeaders() }
    );
  }

  getPaymentStatus(paymentId: number): Observable<any> {
    return this.http.get(`${this.baseUrl}/payments/${paymentId}`, { 
      headers: this.getAuthHeaders() 
    });
  }
}
