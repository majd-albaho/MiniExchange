import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AuthService } from '../../../core/services/auth.service';

interface NavItem {
  label: string;
  icon: string;
  route: string;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, MatIconModule, MatTooltipModule],
  template: `
    <aside class="sidebar" [class.collapsed]="collapsed()">
      <div class="sidebar-logo" (click)="toggleCollapse()">
        <span class="logo-icon">₿</span>
        @if (!collapsed()) {
          <span class="logo-text">MiniExchange</span>
        }
      </div>

      <nav class="sidebar-nav">
        @for (item of navItems; track item.route) {
          <a
            [routerLink]="item.route"
            routerLinkActive="active"
            class="nav-item"
            [matTooltip]="collapsed() ? item.label : ''"
            matTooltipPosition="right"
          >
            <mat-icon>{{ item.icon }}</mat-icon>
            @if (!collapsed()) {
              <span>{{ item.label }}</span>
            }
          </a>
        }
      </nav>

      <div class="sidebar-footer">
        <a class="nav-item logout" (click)="logout()" [matTooltip]="collapsed() ? 'Logout' : ''" matTooltipPosition="right">
          <mat-icon>logout</mat-icon>
          @if (!collapsed()) {
            <span>Logout</span>
          }
        </a>
      </div>
    </aside>
  `,
  styles: [`
    .sidebar {
      width: 220px;
      min-width: 220px;
      background: var(--bg-secondary);
      border-right: 1px solid var(--border-color);
      display: flex;
      flex-direction: column;
      transition: width 0.25s ease, min-width 0.25s ease;
      z-index: 100;
    }
    .sidebar.collapsed {
      width: 64px;
      min-width: 64px;
    }
    .sidebar-logo {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 20px 16px;
      cursor: pointer;
      border-bottom: 1px solid var(--border-color);
      white-space: nowrap;
      overflow: hidden;
    }
    .logo-icon {
      font-size: 1.8rem;
      color: var(--accent);
    }
    .logo-text {
      font-size: 1rem;
      font-weight: 700;
      color: var(--text-primary);
    }
    .sidebar-nav {
      flex: 1;
      display: flex;
      flex-direction: column;
      padding: 12px 8px;
      gap: 4px;
    }
    .nav-item {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 10px 12px;
      border-radius: 8px;
      cursor: pointer;
      text-decoration: none;
      color: var(--text-secondary);
      transition: all 0.15s ease;
      white-space: nowrap;
      overflow: hidden;
      font-size: 0.9rem;
    }
    .nav-item:hover, .nav-item.active {
      background: var(--accent-alpha);
      color: var(--accent);
    }
    .nav-item mat-icon { font-size: 20px; width: 20px; height: 20px; }
    .sidebar-footer {
      padding: 12px 8px;
      border-top: 1px solid var(--border-color);
    }
    .logout { color: var(--danger) !important; }
    .logout:hover { background: rgba(255,82,82,0.12) !important; }
  `],
})
export class SidebarComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  collapsed = signal(false);

  navItems: NavItem[] = [
    { label: 'Home', icon: 'home', route: '/home' },
    { label: 'Wallet', icon: 'account_balance_wallet', route: '/wallet' },
    { label: 'Trade', icon: 'candlestick_chart', route: '/trade' },
    { label: 'Transactions', icon: 'receipt_long', route: '/transactions' },
    { label: 'Settings', icon: 'settings', route: '/settings' },
  ];

  toggleCollapse(): void {
    this.collapsed.update(v => !v);
  }

  logout(): void {
    this.authService.logout();
  }
}
