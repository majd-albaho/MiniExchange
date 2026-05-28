import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, RouterLink,
    MatFormFieldModule, MatInputModule, MatButtonModule,
    MatIconModule, MatCheckboxModule,
  ],
  template: `
    <div class="auth-page">
      <div class="auth-card">
        <div class="auth-logo">
          <span class="logo-symbol">₿</span>
          <h1>MiniExchange</h1>
          <p>Welcome back! Sign in to continue trading.</p>
        </div>

        <form [formGroup]="form" (ngSubmit)="onSubmit()" class="auth-form">
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Email Address</mat-label>
            <mat-icon matPrefix>email</mat-icon>
            <input matInput formControlName="email" type="email" placeholder="you@example.com" />
            @if (form.get('email')?.hasError('required') && form.get('email')?.touched) {
              <mat-error>Email is required</mat-error>
            }
            @if (form.get('email')?.hasError('email') && form.get('email')?.touched) {
              <mat-error>Enter a valid email address</mat-error>
            }
          </mat-form-field>

          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Password</mat-label>
            <mat-icon matPrefix>lock</mat-icon>
            <input matInput formControlName="password" [type]="showPassword() ? 'text' : 'password'" />
            <button mat-icon-button matSuffix type="button" (click)="showPassword.update(v => !v)">
              <mat-icon>{{ showPassword() ? 'visibility_off' : 'visibility' }}</mat-icon>
            </button>
            @if (form.get('password')?.hasError('required') && form.get('password')?.touched) {
              <mat-error>Password is required</mat-error>
            }
          </mat-form-field>

          @if (show2FAField()) {
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>2FA Code</mat-label>
              <mat-icon matPrefix>security</mat-icon>
              <input matInput formControlName="twoFactorCode" placeholder="6-digit code" maxlength="6" />
            </mat-form-field>
          }

          <div class="form-actions">
            <a href="#" class="forgot-link">Forgot Password?</a>
          </div>

          <button mat-raised-button color="primary" type="submit" class="submit-btn" [disabled]="loading()">
            @if (loading()) {
              <span class="btn-spinner"></span> Signing in...
            } @else {
              Sign In
            }
          </button>
        </form>

        <div class="auth-footer">
          <p>Don't have an account? <a routerLink="/auth/signup">Create Account</a></p>
        </div>
      </div>

      <div class="auth-illustration">
        <div class="illustration-content">
          <h2>Trade with Confidence</h2>
          <p>Access global crypto markets with real-time prices, advanced charts, and secure transactions.</p>
          <div class="feature-list">
            <div class="feature">✓ Spot Trading</div>
            <div class="feature">✓ Multi-currency Wallet</div>
            <div class="feature">✓ Secure 2FA Protection</div>
            <div class="feature">✓ Real-time Order Book</div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .auth-page {
      min-height: 100vh;
      display: flex;
      background: var(--bg-primary);
    }
    .auth-card {
      width: 480px;
      min-width: 480px;
      background: var(--bg-secondary);
      border-right: 1px solid var(--border-color);
      padding: 48px 40px;
      display: flex;
      flex-direction: column;
      justify-content: center;
    }
    .auth-logo {
      text-align: center;
      margin-bottom: 36px;
    }
    .logo-symbol {
      font-size: 3rem;
      color: var(--accent);
      display: block;
      margin-bottom: 8px;
    }
    .auth-logo h1 {
      font-size: 1.8rem;
      font-weight: 800;
      color: var(--text-primary);
      margin: 0 0 8px;
    }
    .auth-logo p { color: var(--text-secondary); font-size: 0.9rem; margin: 0; }
    .auth-form { display: flex; flex-direction: column; gap: 4px; }
    .full-width { width: 100%; }
    .form-actions { display: flex; justify-content: flex-end; margin: -4px 0 8px; }
    .forgot-link { font-size: 0.83rem; color: var(--accent); text-decoration: none; }
    .submit-btn { width: 100%; height: 48px; font-size: 1rem; font-weight: 600; margin-top: 8px; }
    .btn-spinner {
      display: inline-block; width: 16px; height: 16px;
      border: 2px solid rgba(255,255,255,0.3); border-top-color: white;
      border-radius: 50%; animation: spin 0.8s linear infinite; margin-right: 8px;
    }
    @keyframes spin { to { transform: rotate(360deg); } }
    .auth-footer {
      text-align: center; margin-top: 24px;
      color: var(--text-secondary); font-size: 0.88rem;
    }
    .auth-footer a { color: var(--accent); text-decoration: none; font-weight: 600; }
    .auth-illustration {
      flex: 1;
      background: linear-gradient(135deg, #1a1f3c 0%, #0d1117 100%);
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 60px;
      position: relative;
      overflow: hidden;
    }
    .auth-illustration::before {
      content: '';
      position: absolute; inset: 0;
      background: radial-gradient(ellipse at 50% 50%, rgba(99, 179, 237, 0.1) 0%, transparent 70%);
    }
    .illustration-content { position: relative; z-index: 1; }
    .illustration-content h2 {
      font-size: 2.2rem; font-weight: 800;
      color: white; margin: 0 0 16px;
    }
    .illustration-content p {
      color: rgba(255,255,255,0.7); font-size: 1rem;
      line-height: 1.6; margin: 0 0 32px; max-width: 400px;
    }
    .feature-list { display: flex; flex-direction: column; gap: 12px; }
    .feature {
      color: rgba(255,255,255,0.9); font-size: 0.95rem;
      padding: 10px 16px; background: rgba(255,255,255,0.05);
      border-radius: 8px; border: 1px solid rgba(255,255,255,0.1);
    }
    @media (max-width: 768px) {
      .auth-illustration { display: none; }
      .auth-card { width: 100%; min-width: 0; }
    }
  `],
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private notif = inject(NotificationService);

  loading = signal(false);
  showPassword = signal(false);
  show2FAField = signal(false);

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
    twoFactorCode: [''],
  });

  async onSubmit(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading.set(true);
    try {
      const { email, password, twoFactorCode } = this.form.value;
      await this.authService.login({
        email: email!,
        password: password!,
        twoFactorCode: twoFactorCode || undefined,
      });
      this.notif.success('Welcome back!');
      this.router.navigate(['/home']);
    } catch (err: any) {
      if (err?.status === 401 && err?.error?.requires2FA) {
        this.show2FAField.set(true);
        this.notif.info('Please enter your 2FA code');
      } else {
        this.notif.error('Login failed. Please check your credentials.');
      }
    } finally {
      this.loading.set(false);
    }
  }
}
