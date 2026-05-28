import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ThemeService } from '../../../core/services/theme.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule, MatMenuModule, RouterLink],
  template: `
    <header class="navbar">
      <div class="navbar-left">
        <div class="market-ticker">
          <span class="ticker-item">BTC <span class="price">$44,187</span> <span class="up">+1.85%</span></span>
          <span class="ticker-item">ETH <span class="price">$2,440</span> <span class="up">+3.12%</span></span>
          <span class="ticker-item">SOL <span class="price">$98.04</span> <span class="up">+5.67%</span></span>
        </div>
      </div>
      <div class="navbar-right">
        <button mat-icon-button (click)="themeService.toggleTheme()" class="icon-btn">
          <mat-icon>{{ themeService.theme() === 'dark' ? 'light_mode' : 'dark_mode' }}</mat-icon>
        </button>
        <button mat-icon-button class="icon-btn notification-btn">
          <mat-icon>notifications</mat-icon>
          <span class="badge">3</span>
        </button>
        <div class="user-menu" [matMenuTriggerFor]="userMenu">
          <div class="avatar">{{ getUserInitials() }}</div>
          <span class="username">{{ authService.user()?.nickname }}</span>
          <mat-icon>expand_more</mat-icon>
        </div>
        <mat-menu #userMenu="matMenu" class="user-dropdown">
          <a mat-menu-item routerLink="/settings">
            <mat-icon>manage_accounts</mat-icon> Profile & Settings
          </a>
          <button mat-menu-item (click)="authService.logout()">
            <mat-icon>logout</mat-icon> Logout
          </button>
        </mat-menu>
      </div>
    </header>
  `,
  styles: [`
    .navbar {
      height: 56px;
      background: var(--bg-secondary);
      border-bottom: 1px solid var(--border-color);
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 0 20px;
      gap: 16px;
    }
    .navbar-left { display: flex; align-items: center; gap: 16px; }
    .market-ticker { display: flex; gap: 20px; }
    .ticker-item { font-size: 0.78rem; color: var(--text-secondary); display: flex; gap: 4px; align-items: center; }
    .price { color: var(--text-primary); font-weight: 600; }
    .up { color: var(--success); }
    .down { color: var(--danger); }
    .navbar-right { display: flex; align-items: center; gap: 8px; }
    .icon-btn { color: var(--text-secondary) !important; }
    .notification-btn { position: relative; }
    .badge {
      position: absolute; top: 6px; right: 6px;
      background: var(--danger); color: white;
      font-size: 0.65rem; width: 16px; height: 16px;
      border-radius: 50%; display: flex; align-items: center; justify-content: center;
    }
    .user-menu {
      display: flex; align-items: center; gap: 8px;
      cursor: pointer; padding: 6px 10px; border-radius: 8px;
      transition: background 0.15s;
    }
    .user-menu:hover { background: var(--accent-alpha); }
    .avatar {
      width: 32px; height: 32px; border-radius: 50%;
      background: var(--accent); color: white;
      display: flex; align-items: center; justify-content: center;
      font-size: 0.75rem; font-weight: 700;
    }
    .username { font-size: 0.85rem; color: var(--text-primary); font-weight: 500; }
    mat-icon { font-size: 18px !important; width: 18px; height: 18px; color: var(--text-secondary); }
  `],
})
export class NavbarComponent {
  authService = inject(AuthService);
  themeService = inject(ThemeService);

  getUserInitials(): string {
    const user = this.authService.user();
    if (!user) return 'U';
    return `${user.firstName[0] ?? ''}${user.lastName[0] ?? ''}`.toUpperCase();
  }
}
