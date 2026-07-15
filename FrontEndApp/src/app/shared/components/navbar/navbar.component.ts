import { Component, inject, computed, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ThemeService } from '../../../core/services/theme.service';
import { MarketService } from '../../../core/services/market.service';

const TICKER_REFRESH_MS = 15000;

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule, MatMenuModule, RouterLink],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css',
})
export class NavbarComponent implements OnInit, OnDestroy {
  authService = inject(AuthService);
  themeService = inject(ThemeService);
  private marketService = inject(MarketService);

  private refreshTimer: ReturnType<typeof setInterval> | null = null;

  // First few tickers (already sorted into preferred order by MarketService) for the header strip.
  readonly strip = computed(() =>
    this.marketService.tickers().slice(0, 3).map(t => ({
      base: t.pair.split('/')[0],
      price: t.price,
      change24h: t.change24h,
    }))
  );

  ngOnInit(): void {
    this.marketService.getTickers();
    this.refreshTimer = setInterval(() => this.marketService.getTickers(), TICKER_REFRESH_MS);
  }

  ngOnDestroy(): void {
    if (this.refreshTimer !== null) {
      clearInterval(this.refreshTimer);
    }
  }

  getUserInitials(): string {
    const user = this.authService.user();
    if (!user) return 'U';
    return `${user.firstName[0] ?? ''}${user.lastName[0] ?? ''}`.toUpperCase();
  }
}
