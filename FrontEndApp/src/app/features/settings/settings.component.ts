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
  template: `
    <div class="settings-page">
      <app-page-header title="Settings" subtitle="Manage your account preferences and security" />

      <div class="settings-layout">
        <!-- Sidebar Nav -->
        <div class="settings-nav">
          @for (tab of settingsTabs; track tab.id) {
            <button class="nav-btn" [class.active]="activeTab() === tab.id" (click)="activeTab.set(tab.id)">
              <mat-icon>{{ tab.icon }}</mat-icon>
              {{ tab.label }}
            </button>
          }
        </div>

        <!-- Content -->
        <div class="settings-content">

          <!-- Profile Tab -->
          @if (activeTab() === 'profile') {
            <div class="settings-card">
              <h3>Profile Information</h3>
              <div class="avatar-section">
                <div class="avatar-large">{{ getUserInitials() }}</div>
                <div>
                  <div class="avatar-name">{{ authService.user()?.firstName }} {{ authService.user()?.lastName }}</div>
                  <div class="avatar-email">{{ authService.user()?.email }}</div>
                  <div class="kyc-badge" [class.verified]="authService.user()?.kycVerified">
                    <mat-icon>{{ authService.user()?.kycVerified ? 'verified' : 'pending' }}</mat-icon>
                    {{ authService.user()?.kycVerified ? 'KYC Verified' : 'KYC Pending' }}
                  </div>
                </div>
              </div>
              <mat-divider />
              <form [formGroup]="profileForm" (ngSubmit)="saveProfile()" class="settings-form">
                <div class="form-row">
                  <mat-form-field appearance="outline">
                    <mat-label>First Name</mat-label>
                    <input matInput formControlName="firstName" />
                  </mat-form-field>
                  <mat-form-field appearance="outline">
                    <mat-label>Last Name</mat-label>
                    <input matInput formControlName="lastName" />
                  </mat-form-field>
                </div>
                <mat-form-field appearance="outline" class="full-width">
                  <mat-label>Nickname</mat-label>
                  <mat-icon matPrefix>alternate_email</mat-icon>
                  <input matInput formControlName="nickname" />
                </mat-form-field>
                <div class="form-actions">
                  <button mat-raised-button color="primary" type="submit" [disabled]="savingProfile()">
                    {{ savingProfile() ? 'Saving...' : 'Save Changes' }}
                  </button>
                </div>
              </form>
            </div>
          }

          <!-- Security Tab -->
          @if (activeTab() === 'security') {
            <!-- Change Password -->
            <div class="settings-card">
              <h3>Change Password</h3>
              <form [formGroup]="passwordForm" (ngSubmit)="changePassword()" class="settings-form">
                <mat-form-field appearance="outline" class="full-width">
                  <mat-label>Current Password</mat-label>
                  <mat-icon matPrefix>lock</mat-icon>
                  <input matInput formControlName="currentPassword" type="password" />
                </mat-form-field>
                <mat-form-field appearance="outline" class="full-width">
                  <mat-label>New Password</mat-label>
                  <mat-icon matPrefix>lock_open</mat-icon>
                  <input matInput formControlName="newPassword" type="password" />
                  @if (passwordForm.get('newPassword')?.hasError('minlength') && passwordForm.get('newPassword')?.touched) {
                    <mat-error>Minimum 8 characters</mat-error>
                  }
                </mat-form-field>
                <mat-form-field appearance="outline" class="full-width">
                  <mat-label>Confirm New Password</mat-label>
                  <mat-icon matPrefix>lock_clock</mat-icon>
                  <input matInput formControlName="confirmPassword" type="password" />
                </mat-form-field>
                <div class="form-actions">
                  <button mat-raised-button color="primary" type="submit" [disabled]="savingPassword()">
                    {{ savingPassword() ? 'Updating...' : 'Update Password' }}
                  </button>
                </div>
              </form>
            </div>

            <!-- 2FA -->
            <div class="settings-card">
              <div class="card-header">
                <div>
                  <h3>Two-Factor Authentication (2FA)</h3>
                  <p class="card-desc">Add an extra layer of security using an authenticator app.</p>
                </div>
                <mat-slide-toggle
                  [checked]="authService.user()?.twoFactorEnabled ?? false"
                  (change)="toggle2FA($event.checked)"
                  color="primary"
                />
              </div>

              @if (show2FASetup()) {
                <div class="twofa-setup">
                  <p>Scan this QR code with your authenticator app (Google Authenticator, Authy, etc.)</p>
                  <div class="qr-wrapper">
                    <qrcode [qrdata]="twoFAData()?.qrCodeUrl ?? ''" [width]="180" [colorDark]="'#ffffff'" [colorLight]="'#1a1f2e'" />
                  </div>
                  <div class="secret-key">
                    <span>Secret Key:</span>
                    <code>{{ twoFAData()?.secret }}</code>
                    <button mat-icon-button (click)="copyText(twoFAData()?.secret ?? '')"><mat-icon>content_copy</mat-icon></button>
                  </div>
                  <mat-form-field appearance="outline" class="full-width">
                    <mat-label>Enter 6-digit verification code</mat-label>
                    <input matInput [(ngModel)]="twoFACode" maxlength="6" placeholder="123456" />
                  </mat-form-field>
                  <button mat-raised-button color="primary" (click)="verify2FA()" [disabled]="twoFACode.length !== 6">
                    Verify & Enable 2FA
                  </button>
                </div>
              }
            </div>

            <!-- Transaction PIN -->
            <div class="settings-card">
              <div class="card-header">
                <div>
                  <h3>Transaction PIN</h3>
                  <p class="card-desc">Set a 6-digit PIN to authorize send transactions.</p>
                </div>
                <span class="status-badge" [class]="authService.user()?.pinEnabled ? 'status-active' : 'status-inactive'">
                  {{ authService.user()?.pinEnabled ? 'Enabled' : 'Not Set' }}
                </span>
              </div>
              <div class="pin-section">
                <mat-form-field appearance="outline">
                  <mat-label>New PIN</mat-label>
                  <input matInput [(ngModel)]="newPin" type="password" maxlength="6" placeholder="6-digit PIN" />
                </mat-form-field>
                <button mat-raised-button color="primary" (click)="setPin()" [disabled]="newPin.length !== 6">
                  {{ authService.user()?.pinEnabled ? 'Update PIN' : 'Set PIN' }}
                </button>
              </div>
            </div>
          }

          <!-- Language Tab -->
          @if (activeTab() === 'language') {
            <div class="settings-card">
              <h3>Language & Region</h3>
              <p class="card-desc">Choose your preferred language for the interface.</p>
              <div class="language-options">
                @for (lang of languages; track lang.code) {
                  <div
                    class="lang-option"
                    [class.selected]="selectedLanguage() === lang.code"
                    (click)="setLanguage(lang.code)"
                  >
                    <span class="lang-flag">{{ lang.flag }}</span>
                    <span class="lang-name">{{ lang.name }}</span>
                    @if (selectedLanguage() === lang.code) {
                      <mat-icon class="check-icon">check_circle</mat-icon>
                    }
                  </div>
                }
              </div>
            </div>
          }

          <!-- Account Tab -->
          @if (activeTab() === 'account') {
            <div class="settings-card">
              <h3>Account Information</h3>
              <div class="info-grid">
                <div class="info-row">
                  <span>User ID</span>
                  <code>{{ authService.user()?.id }}</code>
                </div>
                <div class="info-row">
                  <span>Email</span>
                  <span>{{ authService.user()?.email }}</span>
                </div>
                <div class="info-row">
                  <span>Member Since</span>
                  <span>{{ authService.user()?.createdAt | date:'mediumDate' }}</span>
                </div>
                <div class="info-row">
                  <span>KYC Status</span>
                  <span class="status-badge" [class]="authService.user()?.kycVerified ? 'status-active' : 'status-inactive'">
                    {{ authService.user()?.kycVerified ? 'Verified' : 'Unverified' }}
                  </span>
                </div>
              </div>
            </div>

            <div class="settings-card danger-zone">
              <h3>Danger Zone</h3>
              <div class="danger-item">
                <div>
                  <strong>Logout</strong>
                  <p>Sign out from all devices</p>
                </div>
                <button mat-raised-button color="warn" (click)="logout()">
                  <mat-icon>logout</mat-icon> Logout
                </button>
              </div>
              <mat-divider />
              <div class="danger-item">
                <div>
                  <strong>Delete Account</strong>
                  <p>Permanently delete your account and data</p>
                </div>
                <button mat-stroked-button color="warn">
                  <mat-icon>delete_forever</mat-icon> Delete Account
                </button>
              </div>
            </div>
          }

        </div>
      </div>
    </div>
  `,
  styles: [`
    .settings-page { display: flex; flex-direction: column; gap: 20px; }
    .settings-layout { display: grid; grid-template-columns: 220px 1fr; gap: 20px; }
    .settings-nav {
      background: var(--bg-card); border: 1px solid var(--border-color);
      border-radius: 12px; padding: 8px;
      display: flex; flex-direction: column; gap: 4px;
      height: fit-content;
    }
    .nav-btn {
      display: flex; align-items: center; gap: 10px;
      padding: 10px 14px; border-radius: 8px; border: none;
      background: transparent; color: var(--text-secondary);
      cursor: pointer; font-size: 0.88rem; font-weight: 500;
      transition: all 0.15s; text-align: left; width: 100%;
    }
    .nav-btn:hover, .nav-btn.active { background: var(--accent-alpha); color: var(--accent); }
    .nav-btn mat-icon { font-size: 18px; width: 18px; height: 18px; }
    .settings-content { display: flex; flex-direction: column; gap: 16px; }
    .settings-card {
      background: var(--bg-card); border: 1px solid var(--border-color);
      border-radius: 12px; padding: 24px;
    }
    .settings-card h3 { margin: 0 0 16px; font-size: 1rem; font-weight: 700; color: var(--text-primary); }
    .card-desc { font-size: 0.84rem; color: var(--text-secondary); margin: -8px 0 16px; }
    .card-header { display: flex; align-items: flex-start; justify-content: space-between; margin-bottom: 12px; }
    .avatar-section { display: flex; align-items: center; gap: 16px; margin-bottom: 20px; }
    .avatar-large {
      width: 64px; height: 64px; border-radius: 50%;
      background: var(--accent); color: white;
      display: flex; align-items: center; justify-content: center;
      font-size: 1.4rem; font-weight: 700;
    }
    .avatar-name { font-size: 1.1rem; font-weight: 700; color: var(--text-primary); }
    .avatar-email { font-size: 0.85rem; color: var(--text-secondary); margin-bottom: 6px; }
    .kyc-badge {
      display: inline-flex; align-items: center; gap: 4px;
      padding: 3px 10px; border-radius: 12px; font-size: 0.78rem; font-weight: 600;
      background: rgba(255,152,0,0.15); color: #ff9800;
    }
    .kyc-badge.verified { background: rgba(76,175,80,0.15); color: var(--success); }
    .kyc-badge mat-icon { font-size: 14px; width: 14px; height: 14px; }
    .settings-form { display: flex; flex-direction: column; gap: 8px; margin-top: 16px; }
    .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
    .full-width { width: 100%; }
    .form-actions { display: flex; justify-content: flex-end; gap: 10px; margin-top: 8px; }
    .twofa-setup { display: flex; flex-direction: column; gap: 12px; padding-top: 12px; border-top: 1px solid var(--border-color); }
    .twofa-setup p { font-size: 0.85rem; color: var(--text-secondary); margin: 0; }
    .qr-wrapper { display: flex; justify-content: flex-start; padding: 16px; background: #1a1f2e; border-radius: 10px; width: fit-content; }
    .secret-key { display: flex; align-items: center; gap: 8px; font-size: 0.82rem; color: var(--text-secondary); }
    .secret-key code { font-family: monospace; color: var(--accent); }
    .pin-section { display: flex; gap: 12px; align-items: flex-end; margin-top: 8px; }
    .language-options { display: flex; flex-direction: column; gap: 8px; max-width: 400px; }
    .lang-option {
      display: flex; align-items: center; gap: 12px;
      padding: 14px 16px; border-radius: 10px; border: 2px solid var(--border-color);
      cursor: pointer; transition: all 0.15s;
    }
    .lang-option:hover { border-color: var(--accent); background: var(--accent-alpha); }
    .lang-option.selected { border-color: var(--accent); background: var(--accent-alpha); }
    .lang-flag { font-size: 1.5rem; }
    .lang-name { flex: 1; font-size: 0.95rem; font-weight: 600; color: var(--text-primary); }
    .check-icon { color: var(--accent); }
    .info-grid { display: flex; flex-direction: column; gap: 0; }
    .info-row {
      display: flex; justify-content: space-between; align-items: center;
      padding: 12px 0; border-bottom: 1px solid var(--border-color);
      font-size: 0.88rem;
    }
    .info-row:last-child { border-bottom: none; }
    .info-row span:first-child { color: var(--text-secondary); }
    .info-row span:last-child, .info-row code { color: var(--text-primary); font-family: monospace; }
    .status-badge { font-size: 0.75rem; padding: 3px 10px; border-radius: 12px; font-weight: 600; }
    .status-active { background: rgba(76,175,80,0.15); color: var(--success); }
    .status-inactive { background: rgba(255,152,0,0.15); color: #ff9800; }
    .danger-zone { border-color: rgba(244,67,54,0.3); }
    .danger-zone h3 { color: var(--danger); }
    .danger-item { display: flex; align-items: center; justify-content: space-between; padding: 12px 0; }
    .danger-item div strong { display: block; font-size: 0.9rem; color: var(--text-primary); }
    .danger-item div p { margin: 4px 0 0; font-size: 0.82rem; color: var(--text-secondary); }
    @media (max-width: 768px) {
      .settings-layout { grid-template-columns: 1fr; }
      .settings-nav { flex-direction: row; overflow-x: auto; }
    }
  `],
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
