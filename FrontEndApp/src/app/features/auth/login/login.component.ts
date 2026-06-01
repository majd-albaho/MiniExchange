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
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
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
