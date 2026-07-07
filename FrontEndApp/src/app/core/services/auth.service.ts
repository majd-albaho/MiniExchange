import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { LoginRequest, LoginResponse, RegisterRequest, User } from '../models/user.model';
import { firstValueFrom } from 'rxjs';

const TOKEN_KEY = 'mx_access_token';
const REFRESH_KEY = 'mx_refresh_token';
const USER_KEY = 'mx_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly baseUrl = environment.apiBase.auth;

  private _user = signal<User | null>(this.loadUser());
  readonly user = this._user.asReadonly();
  readonly isAuthenticated = computed(() => !!this._user());

  constructor(private http: HttpClient, private router: Router) {}

  private loadUser(): User | null {
    try {
      const raw = localStorage.getItem(USER_KEY);
      return raw ? JSON.parse(raw) : null;
    } catch {
      return null;
    }
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  async login(payload: LoginRequest): Promise<void> {
    try {
      const res = await firstValueFrom(
        this.http.post<LoginResponse>(`${this.baseUrl}/Auth/Login`, payload)
      );
      this.storeSession(res, this.buildUserFromToken(res.accessToken, payload.email));
    } catch (err) {
      console.error('[AuthService] login error:', err);
      throw err;
    }
  }

  async register(payload: RegisterRequest): Promise<void> {
    try {
      await firstValueFrom(
        this.http.post(`${this.baseUrl}/Auth/Register`, payload)
      );
    } catch (err) {
      console.error('[AuthService] register error:', err);
      throw err;
    }
  }

  async refreshToken(): Promise<void> {
    try {
      const refreshToken = localStorage.getItem(REFRESH_KEY);
      const res = await firstValueFrom(
        this.http.post<LoginResponse>(`${this.baseUrl}/Auth/RefreshToken`, { refreshToken })
      );
      this.storeSession(res, this.buildUserFromToken(res.accessToken, this._user()?.email ?? ''));
    } catch (err) {
      console.error('[AuthService] refresh error:', err);
      this.logout();
    }
  }

  private buildUserFromToken(accessToken: string, fallbackEmail: string): User {
    const claims = this.decodeTokenClaims(accessToken);
    const email = claims['email'] ?? fallbackEmail;
    return {
      id: claims['sub'] ?? '',
      email,
      nickname: email.split('@')[0],
      firstName: '',
      lastName: '',
      twoFactorEnabled: false,
      pinEnabled: false,
      language: 'en',
      createdAt: new Date().toISOString(),
      kycVerified: false,
    };
  }

  private decodeTokenClaims(token: string): Record<string, string> {
    try {
      const payload = token.split('.')[1];
      const json = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
      return JSON.parse(json);
    } catch {
      return {};
    }
  }

  private storeSession(res: LoginResponse, user: User): void {
    localStorage.setItem(TOKEN_KEY, res.accessToken);
    localStorage.setItem(REFRESH_KEY, res.refreshToken);
    localStorage.setItem(USER_KEY, JSON.stringify(user));
    this._user.set(user);
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(REFRESH_KEY);
    localStorage.removeItem(USER_KEY);
    this._user.set(null);
    this.router.navigate(['/auth/login']);
  }

  updateUserLocally(partial: Partial<User>): void {
    const current = this._user();
    if (current) {
      const updated = { ...current, ...partial };
      this._user.set(updated);
      localStorage.setItem(USER_KEY, JSON.stringify(updated));
    }
  }
}
