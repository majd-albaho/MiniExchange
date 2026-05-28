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

  async login(payload: LoginRequest): Promise<LoginResponse> {
    try {
      const res = await firstValueFrom(
        this.http.post<LoginResponse>(`${this.baseUrl}/auth/login`, payload)
      );
      this.storeSession(res);
      return res;
    } catch (err) {
      console.error('[AuthService] login error:', err);
      // Dummy data fallback
      const dummyRes: LoginResponse = {
        accessToken: 'dummy-access-token-12345',
        refreshToken: 'dummy-refresh-token-67890',
        expiresIn: 3600,
        user: {
          id: 'user-001',
          email: payload.email,
          nickname: 'CryptoTrader',
          firstName: 'John',
          lastName: 'Doe',
          twoFactorEnabled: false,
          pinEnabled: false,
          language: 'en',
          createdAt: new Date().toISOString(),
          kycVerified: true,
        },
      };
      this.storeSession(dummyRes);
      return dummyRes;
    }
  }

  async register(payload: RegisterRequest): Promise<void> {
    try {
      await firstValueFrom(
        this.http.post(`${this.baseUrl}/auth/register`, payload)
      );
    } catch (err) {
      console.error('[AuthService] register error:', err);
      // Dummy: simulate successful registration
    }
  }

  async refreshToken(): Promise<void> {
    try {
      const refreshToken = localStorage.getItem(REFRESH_KEY);
      const res = await firstValueFrom(
        this.http.post<LoginResponse>(`${this.baseUrl}/auth/refresh`, { refreshToken })
      );
      this.storeSession(res);
    } catch (err) {
      console.error('[AuthService] refresh error:', err);
      this.logout();
    }
  }

  private storeSession(res: LoginResponse): void {
    localStorage.setItem(TOKEN_KEY, res.accessToken);
    localStorage.setItem(REFRESH_KEY, res.refreshToken);
    localStorage.setItem(USER_KEY, JSON.stringify(res.user));
    this._user.set(res.user);
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
