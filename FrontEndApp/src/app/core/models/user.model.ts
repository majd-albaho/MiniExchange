export interface User {
  id: string;
  email: string;
  nickname: string;
  firstName: string;
  lastName: string;
  twoFactorEnabled: boolean;
  pinEnabled: boolean;
  language: 'en' | 'fr';
  createdAt: string;
  kycVerified: boolean;
  avatarUrl?: string;
}

export interface LoginRequest {
  email: string;
  password: string;
  twoFactorCode?: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  user: User;
}

export interface RegisterRequest {
  email: string;
  password: string;
  confirmPassword: string;
  firstName: string;
  lastName: string;
  nickname: string;
}
