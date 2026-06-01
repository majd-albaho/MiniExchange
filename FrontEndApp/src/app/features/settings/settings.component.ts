import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatTabsModule } from '@angular/material/tabs';
import { MatSelectModule } from '@angular/material/select';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatDividerModule } from '@angular/material/divider';
import { QRCodeComponent } from 'angularx-qrcode';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { UserService, Setup2FAResponse } from '../../core/services/user.service';
import { NotificationService } from '../../core/services/notification.service';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, FormsModule,
    MatButtonModule, MatIconModule, MatFormFieldModule, MatInputModule,
    MatSlideToggleModule, MatTabsModule, MatSelectModule,
    MatDialogModule, MatDividerModule, QRCodeComponent,
    PageHeaderComponent,
  ],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.css',
})
export class SettingsComponent implements OnInit {
  authService = inject(AuthService);
  private userService = inject(UserService);
  private notif = inject(NotificationService);
  private fb = inject(FormBuilder);

  activeTab = signal('profile');
  savingProfile = signal(false);
  savingPassword = signal(false);
  show2FASetup = signal(false);
  twoFAData = signal<Setup2FAResponse | null>(null);
  twoFACode = '';
  newPin = '';
  selectedLanguage = signal<'en' | 'fr'>('en');

  settingsTabs = [
    { id: 'profile', label: 'Profile', icon: 'person' },
    { id: 'security', label: 'Security', icon: 'security' },
    { id: 'language', label: 'Language', icon: 'language' },
    { id: 'account', label: 'Account', icon: 'manage_accounts' },
  ];

  languages = [
    { code: 'en' as 'en' | 'fr', name: 'English', flag: '🇺🇸' },
    { code: 'fr' as 'en' | 'fr', name: 'Français', flag: '🇫🇷' },
  ];

  profileForm = this.fb.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    nickname: ['', Validators.required],
  });

  passwordForm = this.fb.group({
    currentPassword: ['', Validators.required],
    newPassword: ['', [Validators.required, Validators.minLength(8)]],
    confirmPassword: ['', Validators.required],
  });

  ngOnInit(): void {
    const user = this.authService.user();
    if (user) {
      this.profileForm.patchValue({
        firstName: user.firstName,
        lastName: user.lastName,
        nickname: user.nickname,
      });
      this.selectedLanguage.set(user.language);
    }
  }

  getUserInitials(): string {
    const u = this.authService.user();
    return u ? `${u.firstName[0] ?? ''}${u.lastName[0] ?? ''}`.toUpperCase() : 'U';
  }

  async saveProfile(): Promise<void> {
    if (this.profileForm.invalid) { this.profileForm.markAllAsTouched(); return; }
    this.savingProfile.set(true);
    try {
      const userId = this.authService.user()!.id;
      await this.userService.updateProfile(userId, this.profileForm.value as any);
      this.notif.success('Profile updated successfully!');
    } catch {
      this.notif.error('Failed to update profile');
    } finally {
      this.savingProfile.set(false);
    }
  }

  async changePassword(): Promise<void> {
    if (this.passwordForm.invalid) { this.passwordForm.markAllAsTouched(); return; }
    const { newPassword, confirmPassword } = this.passwordForm.value;
    if (newPassword !== confirmPassword) {
      this.notif.error('Passwords do not match');
      return;
    }
    this.savingPassword.set(true);
    try {
      await this.userService.changePassword(this.authService.user()!.id, this.passwordForm.value as any);
      this.notif.success('Password updated successfully!');
      this.passwordForm.reset();
    } catch {
      this.notif.error('Failed to update password');
    } finally {
      this.savingPassword.set(false);
    }
  }

  async toggle2FA(enable: boolean): Promise<void> {
    if (enable) {
      const data = await this.userService.setup2FA(this.authService.user()!.id);
      this.twoFAData.set(data);
      this.show2FASetup.set(true);
    } else {
      await this.userService.disable2FA(this.authService.user()!.id, '');
      this.show2FASetup.set(false);
      this.notif.info('2FA has been disabled');
    }
  }

  async verify2FA(): Promise<void> {
    const ok = await this.userService.verify2FA(this.authService.user()!.id, this.twoFACode);
    if (ok) {
      this.notif.success('2FA enabled successfully!');
      this.show2FASetup.set(false);
    } else {
      this.notif.error('Invalid 2FA code');
    }
  }

  async setPin(): Promise<void> {
    if (this.newPin.length !== 6) return;
    await this.userService.setPin(this.authService.user()!.id, this.newPin);
    this.notif.success('Transaction PIN set successfully!');
    this.newPin = '';
  }

  async setLanguage(lang: 'en' | 'fr'): Promise<void> {
    this.selectedLanguage.set(lang);
    await this.userService.updateLanguage(this.authService.user()!.id, lang);
    this.notif.success(`Language changed to ${lang === 'en' ? 'English' : 'Français'}`);
  }

  copyText(text: string): void {
    navigator.clipboard.writeText(text);
    this.notif.success('Copied to clipboard!');
  }

  logout(): void {
    this.authService.logout();
  }
}
