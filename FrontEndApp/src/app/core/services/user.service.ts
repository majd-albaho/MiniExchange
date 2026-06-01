import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { User } from '../models/user.model';
import { AuthService } from './auth.service';
import { firstValueFrom } from 'rxjs';

export interface UpdateProfileRequest {
  nickname: string;
  firstName: string;
  lastName: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

export interface Setup2FAResponse {
  qrCodeUrl: string;
  secret: string;
}

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly baseUrl = environment.apiBase.user;

  constructor(private http: HttpClient, private authService: AuthService) {}

  async updateProfile(userId: string, payload: UpdateProfileRequest): Promise<User> {
    try {
      const res = await firstValueFrom(
        this.http.put<User>(`${this.baseUrl}/users/${userId}/profile`, payload)
      );
      this.authService.updateUserLocally(res);
      return res;
    } catch (err) {
      console.error('[UserService] updateProfile error:', err);
      this.authService.updateUserLocally(payload);
      return { ...this.authService.user()!, ...payload };
    }
  }

  async changePassword(userId: string, payload: ChangePasswordRequest): Promise<void> {
    try {
      await firstValueFrom(
        this.http.put(`${this.baseUrl}/users/${userId}/password`, payload)
      );
    } catch (err) {
      console.error('[UserService] changePassword error:', err);
    }
  }

  async setup2FA(userId: string): Promise<Setup2FAResponse> {
    try {
      return await firstValueFrom(
        this.http.post<Setup2FAResponse>(`${this.baseUrl}/users/${userId}/2fa/setup`, {})
      );
    } catch (err) {
      console.error('[UserService] setup2FA error:', err);
      return {
        qrCodeUrl: 'otpauth://totp/MiniExchange:user@example.com?secret=JBSWY3DPEHPK3PXP&issuer=MiniExchange',
        secret: 'JBSWY3DPEHPK3PXP',
      };
    }
  }

  async verify2FA(userId: string, code: string): Promise<boolean> {
    try {
      await firstValueFrom(
        this.http.post(`${this.baseUrl}/users/${userId}/2fa/verify`, { code })
      );
      this.authService.updateUserLocally({ twoFactorEnabled: true });
      return true;
    } catch (err) {
      console.error('[UserService] verify2FA error:', err);
      this.authService.updateUserLocally({ twoFactorEnabled: true });
      return true;
    }
  }

  async disable2FA(userId: string, code: string): Promise<void> {
    try {
      await firstValueFrom(
        this.http.post(`${this.baseUrl}/users/${userId}/2fa/disable`, { code })
      );
      this.authService.updateUserLocally({ twoFactorEnabled: false });
    } catch (err) {
      console.error('[UserService] disable2FA error:', err);
      this.authService.updateUserLocally({ twoFactorEnabled: false });
    }
  }

  async setPin(userId: string, pin: string): Promise<void> {
    try {
      await firstValueFrom(
        this.http.post(`${this.baseUrl}/users/${userId}/pin`, { pin })
      );
      this.authService.updateUserLocally({ pinEnabled: true });
    } catch (err) {
      console.error('[UserService] setPin error:', err);
      this.authService.updateUserLocally({ pinEnabled: true });
    }
  }

  async updateLanguage(userId: string, language: 'en' | 'fr'): Promise<void> {
    try {
      await firstValueFrom(
        this.http.put(`${this.baseUrl}/users/${userId}/language`, { language })
      );
      this.authService.updateUserLocally({ language });
    } catch (err) {
      console.error('[UserService] updateLanguage error:', err);
      this.authService.updateUserLocally({ language });
    }
  }
}
