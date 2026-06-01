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
  templateUrl: './signup.component.html',
  styleUrl: './signup.component.css',
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
