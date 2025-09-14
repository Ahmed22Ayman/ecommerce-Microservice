import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { CheckboxModule } from 'primeng/checkbox';
import { FormsModule } from '@angular/forms';
import { DividerModule } from 'primeng/divider';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [RouterLink, ButtonModule, CardModule, InputTextModule, PasswordModule, CheckboxModule, FormsModule, DividerModule],
  template: `
    <div class="login-container">
      <p-card>
        <ng-template pTemplate="header">
          <div class="login-header">
            <h2>Welcome Back</h2>
            <p>Sign in to your account</p>
          </div>
        </ng-template>
        
        <form class="login-form" (ngSubmit)="onLogin()">
          <div class="field">
            <label for="email">Email</label>
            <input 
              id="email" 
              type="email" 
              pInputText 
              [(ngModel)]="loginData.email" 
              name="email"
              placeholder="Enter your email"
              class="w-full">
          </div>
          
          <div class="field">
            <label for="password">Password</label>
            <p-password 
              id="password"
              [(ngModel)]="loginData.password" 
              name="password"
              placeholder="Enter your password"
              [toggleMask]="true"
              [feedback]="false"
              styleClass="w-full">
            </p-password>
          </div>
          
          <div class="field-checkbox">
            <p-checkbox 
              [(ngModel)]="loginData.rememberMe" 
              name="rememberMe" 
              inputId="rememberMe">
            </p-checkbox>
            <label for="rememberMe">Remember me</label>
          </div>
          
          <p-button 
            type="submit" 
            label="Sign In" 
            icon="pi pi-sign-in" 
            styleClass="w-full"
            [loading]="isLoading">
          </p-button>
        </form>
        
        <p-divider></p-divider>
        
        <div class="login-footer">
          <p>Don't have an account? <a href="#" class="text-primary">Sign up</a></p>
          <p><a href="#" class="text-primary">Forgot your password?</a></p>
        </div>
        
        <ng-template pTemplate="footer">
          <div class="text-center">
            <p-button 
              routerLink="/" 
              label="Back to Home" 
              icon="pi pi-home" 
              severity="secondary"
              [text]="true">
            </p-button>
          </div>
        </ng-template>
      </p-card>
    </div>
  `,
  styles: [`
    .login-container {
      max-width: 400px;
      margin: 2rem auto;
    }
    .login-header {
      text-align: center;
      padding: 2rem 2rem 0;
    }
    .login-header h2 {
      margin: 0 0 0.5rem 0;
      color: var(--primary-color);
    }
    .login-header p {
      margin: 0;
      color: var(--text-color-secondary);
    }
    .login-form {
      padding: 2rem;
    }
    .field {
      margin-bottom: 1.5rem;
    }
    .field label {
      display: block;
      margin-bottom: 0.5rem;
      font-weight: 500;
    }
    .field-checkbox {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      margin-bottom: 1.5rem;
    }
    .login-footer {
      text-align: center;
      padding: 0 2rem 2rem;
    }
    .login-footer p {
      margin: 0.5rem 0;
    }
    .text-primary {
      color: var(--primary-color);
      text-decoration: none;
    }
    .text-primary:hover {
      text-decoration: underline;
    }
  `]
})
export class LoginComponent {
  loginData = {
    email: '',
    password: '',
    rememberMe: false
  };
  
  isLoading = false;

  onLogin() {
    this.isLoading = true;
    
    // Simulate API call
    setTimeout(() => {
      console.log('Login attempt:', this.loginData);
      this.isLoading = false;
      // TODO: Implement actual authentication with Auth Service
    }, 1500);
  }
}
