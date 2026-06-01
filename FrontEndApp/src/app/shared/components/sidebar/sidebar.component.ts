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
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.css',
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
