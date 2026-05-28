import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatStepperModule } from '@angular/material/stepper';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';

function passwordMatchValidator(control: AbstractControl) {
  const parent = control.parent;
  if (!parent) return null;
  const pw = parent.get('password')?.value;
  return control.value === pw ? null : { mismatch: true };
}

@Component({
  selector: 'app-signup',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, RouterLink,
    MatFormFieldModule, MatInputModule, MatButtonModule,
    MatIconModule, MatStepperModule, MatCheckboxModule,
  ],
  template: `
    <div class="auth-page">
      <div class="auth-card wide">
        <div class="auth-logo">
          <span class="logo-symbol">₿</span>
          <h1>Create Account</h1>
          <p>Join MiniExchange and start trading today.</p>
        </div>

        <mat-stepper linear #stepper class="signup-stepper">

          <!-- Step 1: Account Info -->
          <mat-step [stepControl]="accountForm" label="Account">
            <form [formGroup]="accountForm" class="step-form">
              <div class="form-row">
                <mat-form-field appearance="outline">
                  <mat-label>First Name</mat-label>
                  <input matInput formControlName="firstName" />
                  @if (accountForm.get('firstName')?.hasError('required') && accountForm.get('firstName')?.touched) {
                    <mat-error>Required</mat-error>
                  }
                </mat-form-field>
                <mat-form-field appearance="outline">
                  <mat-label>Last Name</mat-label>
                  <input matInput formControlName="lastName" />
                  @if (accountForm.get('lastName')?.hasError('required') && accountForm.get('lastName')?.touched) {
                    <mat-error>Required</mat-error>
                  }
                </mat-form-field>
              </div>

              <mat-form-field appearance="outline" class="full-width">
                <mat-label>Nickname / Display Name</mat-label>
                <mat-icon matPrefix>alternate_email</mat-icon>
                <input matInput formControlName="nickname" />
                @if (accountForm.get('nickname')?.hasError('required') && accountForm.get('nickname')?.touched) {
                  <mat-error>Nickname is required</mat-error>
                }
              </mat-form-field>

              <mat-form-field appearance="outline" class="full-width">
                <mat-label>Email Address</mat-label>
                <mat-icon matPrefix>email</mat-icon>
                <input matInput formControlName="email" type="email" />
                @if (accountForm.get('email')?.hasError('required') && accountForm.get('email')?.touched) {
                  <mat-error>Email is required</mat-error>
                }
                @if (accountForm.get('email')?.hasError('email') && accountForm.get('email')?.touched) {
                  <mat-error>Invalid email address</mat-error>
                }
              </mat-form-field>

              <div class="step-actions">
                <button mat-raised-button color="primary" matStepperNext type="button"
                  (click)="accountForm.markAllAsTouched()">
                  Continue
                </button>
              </div>
            </form>
          </mat-step>

          <!-- Step 2: Password -->
          <mat-step [stepControl]="passwordForm" label="Security">
            <form [formGroup]="passwordForm" class="step-form">
              <mat-form-field appearance="outline" class="full-width">
                <mat-label>Password</mat-label>
                <mat-icon matPrefix>lock</mat-icon>
                <input matInput formControlName="password" [type]="showPw() ? 'text' : 'password'" />
                <button mat-icon-button matSuffix type="button" (click)="showPw.update(v => !v)">
                  <mat-icon>{{ showPw() ? 'visibility_off' : 'visibility' }}</mat-icon>
                </button>
                @if (passwordForm.get('password')?.hasError('minlength') && passwordForm.get('password')?.touched) {
                  <mat-error>Minimum 8 characters</mat-error>
                }
              </mat-form-field>

              <mat-form-field appearance="outline" class="full-width">
                <mat-label>Confirm Password</mat-label>
                <mat-icon matPrefix>lock_clock</mat-icon>
                <input matInput formControlName="confirmPassword" [type]="showConfirmPw() ? 'text' : 'password'" />
                <button mat-icon-button matSuffix type="button" (click)="showConfirmPw.update(v => !v)">
                  <mat-icon>{{ showConfirmPw() ? 'visibility_off' : 'visibility' }}</mat-icon>
                </button>
                @if (passwordForm.get('confirmPassword')?.hasError('mismatch') && passwordForm.get('confirmPassword')?.touched) {
                  <mat-error>Passwords do not match</mat-error>
                }
              </mat-form-field>

              <div class="password-strength">
                <div class="strength-label">Password strength:</div>
                <div class="strength-bars">
                  @for (s of [1,2,3,4]; track s) {
                    <div class="bar" [class.filled]="s <= passwordStrength()"></div>
                  }
                </div>
                <span class="strength-text">{{ strengthLabel() }}</span>
              </div>

              <div class="step-actions">
                <button mat-button matStepperPrevious type="button">Back</button>
                <button mat-raised-button color="primary" matStepperNext type="button"
                  (click)="passwordForm.markAllAsTouched()">
                  Continue
                </button>
              </div>
            </form>
          </mat-step>

          <!-- Step 3: Confirm -->
          <mat-step label="Confirm">
            <div class="confirm-step">
              <div class="confirm-summary">
                <h3>Review your details</h3>
                <div class="summary-item">
                  <span>Name:</span>
                  <strong>{{ accountForm.value.firstName }} {{ accountForm.value.lastName }}</strong>
                </div>
                <div class="summary-item">
                  <span>Nickname:</span>
                  <strong>{{ accountForm.value.nickname }}</strong>
                </div>
                <div class="summary-item">
                  <span>Email:</span>
                  <strong>{{ accountForm.value.email }}</strong>
                </div>
              </div>

              <mat-checkbox [checked]="agreedToTerms" (change)="agreedToTerms = $event.checked">
                I agree to the <a href="#">Terms of Service</a> and <a href="#">Privacy Policy</a>
              </mat-checkbox>

              <div class="step-actions">
                <button mat-button matStepperPrevious>Back</button>
                <button mat-raised-button color="primary" (click)="onSubmit()" [disabled]="loading() || !agreedToTerms">
                  @if (loading()) {
                    <span class="btn-spinner"></span> Creating account...
                  } @else {
                    Create Account
                  }
                </button>
              </div>
            </div>
          </mat-step>
        </mat-stepper>

        <div class="auth-footer">
          <p>Already have an account? <a routerLink="/auth/login">Sign In</a></p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .auth-page {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: var(--bg-primary);
      padding: 24px;
    }
    .auth-card {
      background: var(--bg-secondary);
      border: 1px solid var(--border-color);
      border-radius: 16px;
      padding: 40px;
      width: 100%;
      max-width: 560px;
    }
    .auth-logo { text-align: center; margin-bottom: 28px; }
    .logo-symbol { font-size: 2.5rem; color: var(--accent); display: block; margin-bottom: 8px; }
    .auth-logo h1 { font-size: 1.6rem; font-weight: 800; color: var(--text-primary); margin: 0 0 6px; }
    .auth-logo p { color: var(--text-secondary); font-size: 0.88rem; margin: 0; }
    .step-form { display: flex; flex-direction: column; gap: 4px; padding-top: 16px; }
    .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
    .full-width { width: 100%; }
    .step-actions { display: flex; justify-content: flex-end; gap: 10px; margin-top: 16px; }
    .confirm-step { padding-top: 16px; display: flex; flex-direction: column; gap: 20px; }
    .confirm-summary { background: var(--bg-primary); border-radius: 10px; padding: 20px; border: 1px solid var(--border-color); }
    .confirm-summary h3 { margin: 0 0 16px; color: var(--text-primary); font-size: 1rem; }
    .summary-item { display: flex; justify-content: space-between; padding: 8px 0; border-bottom: 1px solid var(--border-color); font-size: 0.88rem; color: var(--text-secondary); }
    .summary-item strong { color: var(--text-primary); }
    .password-strength { display: flex; align-items: center; gap: 8px; margin-top: 4px; }
    .strength-label { font-size: 0.78rem; color: var(--text-secondary); }
    .strength-bars { display: flex; gap: 4px; }
    .bar { width: 36px; height: 4px; border-radius: 2px; background: var(--border-color); transition: background 0.2s; }
    .bar.filled:nth-child(1) { background: #f44336; }
    .bar.filled:nth-child(2) { background: #ff9800; }
    .bar.filled:nth-child(3) { background: #ffeb3b; }
    .bar.filled:nth-child(4) { background: #4caf50; }
    .strength-text { font-size: 0.78rem; color: var(--text-secondary); }
    .auth-footer { text-align: center; margin-top: 20px; color: var(--text-secondary); font-size: 0.88rem; }
    .auth-footer a { color: var(--accent); text-decoration: none; font-weight: 600; }
    .btn-spinner {
      display: inline-block; width: 14px; height: 14px;
      border: 2px solid rgba(255,255,255,0.3); border-top-color: white;
      border-radius: 50%; animation: spin 0.8s linear infinite; margin-right: 8px;
    }
    @keyframes spin { to { transform: rotate(360deg); } }
  `],
})
export class SignupComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private notif = inject(NotificationService);

  loading = signal(false);
  showPw = signal(false);
  showConfirmPw = signal(false);
  agreedToTerms = false;

  accountForm = this.fb.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    nickname: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
  });

  passwordForm = this.fb.group({
    password: ['', [Validators.required, Validators.minLength(8)]],
    confirmPassword: ['', [Validators.required, passwordMatchValidator]],
  });

  passwordStrength = signal(0);
  strengthLabel = signal('');

  constructor() {
    this.passwordForm.get('password')?.valueChanges.subscribe(pw => {
      const strength = this.calcStrength(pw ?? '');
      this.passwordStrength.set(strength);
      this.strengthLabel.set(['', 'Weak', 'Fair', 'Good', 'Strong'][strength]);
    });
  }

  private calcStrength(pw: string): number {
    let score = 0;
    if (pw.length >= 8) score++;
    if (/[A-Z]/.test(pw)) score++;
    if (/[0-9]/.test(pw)) score++;
    if (/[^A-Za-z0-9]/.test(pw)) score++;
    return score;
  }

  async onSubmit(): Promise<void> {
    if (!this.agreedToTerms) return;
    this.loading.set(true);
    try {
      const { firstName, lastName, nickname, email } = this.accountForm.value;
      const { password, confirmPassword } = this.passwordForm.value;
      await this.authService.register({
        firstName: firstName!,
        lastName: lastName!,
        nickname: nickname!,
        email: email!,
        password: password!,
        confirmPassword: confirmPassword!,
      });
      this.notif.success('Account created successfully! Please sign in.');
      this.router.navigate(['/auth/login']);
    } catch {
      this.notif.error('Registration failed. Please try again.');
    } finally {
      this.loading.set(false);
    }
  }
}
