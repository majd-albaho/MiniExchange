import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { AuthService } from '../../core/services/auth.service';
import { WalletService } from '../../core/services/wallet.service';
import { MarketService } from '../../core/services/market.service';
import { TransactionService } from '../../core/services/transaction.service';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { StatCardComponent } from '../../shared/components/stat-card/stat-card.component';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';
import { WalletOverview } from '../../core/models/wallet.model';
import { MarketTicker } from '../../core/models/trade.model';
import { Transaction } from '../../core/models/transaction.model';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    CommonModule, RouterLink, MatButtonModule, MatIconModule,
    MatTableModule, PageHeaderComponent, StatCardComponent, LoadingSpinnerComponent,
  ],
  template: `
    <div class="home-page">
      <app-page-header
        [title]="'Welcome back, ' + (authService.user()?.nickname ?? 'Trader') + '! 👋'"
        subtitle="Here's your portfolio overview for today."
      >
        <button mat-raised-button color="primary" routerLink="/trade">
          <mat-icon>candlestick_chart</mat-icon> Start Trading
        </button>
      </app-page-header>

      @if (loading()) {
        <app-loading-spinner message="Loading your dashboard..." />
      } @else {
        <!-- Portfolio Stats -->
        <div class="stats-grid">
          <app-stat-card
            label="Total Portfolio Value"
            [value]="'$' + (wallet()?.totalBalanceUSDT | number:'1.2-2')"
            [change]="wallet()?.totalChange24h"
          />
          <app-stat-card
            label="Assets"
            [value]="(wallet()?.assets?.length ?? 0) + ' currencies'"
          />
          <app-stat-card
            label="24h Market Mood"
            value="Bullish 📈"
          />
          <app-stat-card
            label="Open Orders"
            value="0"
          />
        </div>

        <!-- Market Overview + Quick Actions -->
        <div class="dashboard-grid">

          <!-- Left: Quick Actions -->
          <div class="dashboard-panel">
            <h3 class="panel-title">Quick Actions</h3>
            <div class="quick-actions">
              <a class="action-btn" routerLink="/wallet">
                <mat-icon>account_balance_wallet</mat-icon>
                <span>Wallet</span>
              </a>
              <a class="action-btn" routerLink="/trade">
                <mat-icon>show_chart</mat-icon>
                <span>Trade</span>
              </a>
              <a class="action-btn" routerLink="/transactions">
                <mat-icon>receipt_long</mat-icon>
                <span>History</span>
              </a>
              <a class="action-btn" routerLink="/settings">
                <mat-icon>settings</mat-icon>
                <span>Settings</span>
              </a>
            </div>
          </div>

          <!-- Center: Portfolio Breakdown -->
          <div class="dashboard-panel portfolio-panel">
            <h3 class="panel-title">Portfolio Breakdown</h3>
            <div class="asset-list">
              @for (asset of wallet()?.assets; track asset.id) {
                <div class="asset-row" routerLink="/wallet">
                  <img [src]="asset.logoUrl" [alt]="asset.symbol" class="asset-logo"
                    onerror="this.src='https://via.placeholder.com/32'" />
                  <div class="asset-info">
                    <div class="asset-name">{{ asset.name }}</div>
                    <div class="asset-amount">{{ asset.balance | number:'1.4-8' }} {{ asset.symbol }}</div>
                  </div>
                  <div class="asset-value">
                    <div class="asset-usdt">\${{ asset.balanceUSDT | number:'1.2-2' }}</div>
                    <div class="asset-change" [class.up]="asset.change24h >= 0" [class.down]="asset.change24h < 0">
                      {{ asset.change24h >= 0 ? '+' : '' }}{{ asset.change24h | number:'1.2-2' }}%
                    </div>
                  </div>
                </div>
              }
            </div>
          </div>

          <!-- Right: Market Tickers -->
          <div class="dashboard-panel">
            <div class="panel-header">
              <h3 class="panel-title">Markets</h3>
              <a routerLink="/trade" class="see-all">See All</a>
            </div>
            <div class="ticker-list">
              @for (ticker of tickers(); track ticker.pair) {
                <div class="ticker-row" [routerLink]="['/trade']">
                  <div class="ticker-pair">{{ ticker.pair }}</div>
                  <div class="ticker-price">\${{ ticker.price | number:'1.2-6' }}</div>
                  <div class="ticker-change" [class.up]="ticker.change24h >= 0" [class.down]="ticker.change24h < 0">
                    {{ ticker.change24h >= 0 ? '+' : '' }}{{ ticker.change24h | number:'1.2-2' }}%
                  </div>
                </div>
              }
            </div>
          </div>
        </div>

        <!-- Recent Transactions -->
        <div class="dashboard-panel full-width">
          <div class="panel-header">
            <h3 class="panel-title">Recent Transactions</h3>
            <a routerLink="/transactions" class="see-all">View All</a>
          </div>
          @if (recentTx().length === 0) {
            <div class="empty-state">
              <mat-icon>receipt_long</mat-icon>
              <p>No transactions yet</p>
            </div>
          } @else {
            <div class="tx-list">
              @for (tx of recentTx(); track tx.id) {
                <div class="tx-row">
                  <div class="tx-icon" [class]="'tx-' + tx.type">
                    <mat-icon>{{ txIconMap[tx.type] }}</mat-icon>
                  </div>
                  <div class="tx-info">
                    <div class="tx-type">{{ tx.type | titlecase }}</div>
                    <div class="tx-date">{{ tx.createdAt | date:'MMM d, h:mm a' }}</div>
                  </div>
                  <div class="tx-amount">
                    <div [class]="tx.type === 'receive' || tx.type === 'buy' ? 'up' : 'down'">
                      {{ tx.type === 'receive' || tx.type === 'buy' ? '+' : '-' }}{{ tx.amount | number:'1.4-8' }} {{ tx.symbol }}
                    </div>
                    <div class="tx-usdt">\${{ tx.amountUSDT | number:'1.2-2' }}</div>
                  </div>
                  <div class="tx-status" [class]="'status-' + tx.status">{{ tx.status }}</div>
                </div>
              }
            </div>
          }
        </div>

        <!-- Announcement Banner -->
        <div class="announcement-banner">
          <mat-icon>campaign</mat-icon>
          <div>
            <strong>New feature:</strong> Limit orders are now available on all USDT pairs. Trade smarter with precise entry prices.
          </div>
          <button mat-button routerLink="/trade">Try Now</button>
        </div>
      }
    </div>
  `,
  styles: [`
    .home-page { display: flex; flex-direction: column; gap: 20px; }
    .stats-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 16px; }
    .dashboard-grid { display: grid; grid-template-columns: 200px 1fr 280px; gap: 16px; }
    .dashboard-panel {
      background: var(--bg-card); border: 1px solid var(--border-color);
      border-radius: 12px; padding: 20px;
    }
    .panel-title { font-size: 0.9rem; font-weight: 700; color: var(--text-primary); margin: 0 0 16px; text-transform: uppercase; letter-spacing: 0.5px; }
    .panel-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 16px; }
    .panel-header .panel-title { margin: 0; }
    .see-all { font-size: 0.8rem; color: var(--accent); text-decoration: none; }
    .full-width { grid-column: 1 / -1; }
    .quick-actions { display: flex; flex-direction: column; gap: 8px; }
    .action-btn {
      display: flex; align-items: center; gap: 10px;
      padding: 10px 14px; border-radius: 8px; text-decoration: none;
      color: var(--text-secondary); font-size: 0.88rem;
      transition: all 0.15s; cursor: pointer;
    }
    .action-btn:hover { background: var(--accent-alpha); color: var(--accent); }
    .action-btn mat-icon { font-size: 18px; width: 18px; height: 18px; }
    .asset-list { display: flex; flex-direction: column; gap: 12px; }
    .asset-row {
      display: flex; align-items: center; gap: 12px;
      padding: 10px; border-radius: 8px; cursor: pointer;
      transition: background 0.15s;
    }
    .asset-row:hover { background: var(--bg-primary); }
    .asset-logo { width: 32px; height: 32px; border-radius: 50%; object-fit: contain; }
    .asset-info { flex: 1; }
    .asset-name { font-size: 0.88rem; font-weight: 600; color: var(--text-primary); }
    .asset-amount { font-size: 0.78rem; color: var(--text-secondary); }
    .asset-value { text-align: right; }
    .asset-usdt { font-size: 0.9rem; font-weight: 600; color: var(--text-primary); }
    .asset-change { font-size: 0.78rem; }
    .ticker-list { display: flex; flex-direction: column; gap: 4px; }
    .ticker-row {
      display: flex; align-items: center; justify-content: space-between;
      padding: 8px 10px; border-radius: 6px; cursor: pointer;
      font-size: 0.85rem; transition: background 0.15s;
    }
    .ticker-row:hover { background: var(--bg-primary); }
    .ticker-pair { font-weight: 600; color: var(--text-primary); flex: 1; }
    .ticker-price { color: var(--text-primary); flex: 1; text-align: center; }
    .ticker-change { font-weight: 600; min-width: 60px; text-align: right; }
    .up { color: var(--success); }
    .down { color: var(--danger); }
    .tx-list { display: flex; flex-direction: column; gap: 8px; }
    .tx-row {
      display: flex; align-items: center; gap: 14px;
      padding: 12px; border-radius: 8px; background: var(--bg-primary);
    }
    .tx-icon {
      width: 36px; height: 36px; border-radius: 50%;
      display: flex; align-items: center; justify-content: center;
    }
    .tx-icon mat-icon { font-size: 18px; }
    .tx-receive, .tx-buy { background: rgba(76, 175, 80, 0.15); color: var(--success); }
    .tx-send, .tx-sell { background: rgba(244, 67, 54, 0.15); color: var(--danger); }
    .tx-info { flex: 1; }
    .tx-type { font-size: 0.88rem; font-weight: 600; color: var(--text-primary); text-transform: capitalize; }
    .tx-date { font-size: 0.76rem; color: var(--text-secondary); }
    .tx-amount { text-align: right; font-size: 0.88rem; font-weight: 600; }
    .tx-usdt { font-size: 0.76rem; color: var(--text-secondary); }
    .tx-status { font-size: 0.76rem; padding: 3px 8px; border-radius: 12px; font-weight: 600; }
    .status-completed { background: rgba(76,175,80,0.15); color: var(--success); }
    .status-pending { background: rgba(255,152,0,0.15); color: #ff9800; }
    .status-failed { background: rgba(244,67,54,0.15); color: var(--danger); }
    .status-filled { background: rgba(76,175,80,0.15); color: var(--success); }
    .empty-state { display: flex; flex-direction: column; align-items: center; padding: 32px; gap: 8px; color: var(--text-secondary); }
    .empty-state mat-icon { font-size: 40px; width: 40px; height: 40px; opacity: 0.4; }
    .announcement-banner {
      display: flex; align-items: center; gap: 14px;
      background: linear-gradient(135deg, rgba(99,179,237,0.15) 0%, rgba(99,179,237,0.05) 100%);
      border: 1px solid rgba(99,179,237,0.3);
      border-radius: 12px; padding: 16px 20px;
      color: var(--text-primary); font-size: 0.88rem;
    }
    .announcement-banner mat-icon { color: var(--accent); }
    .announcement-banner div { flex: 1; }
    @media (max-width: 1024px) {
      .stats-grid { grid-template-columns: repeat(2, 1fr); }
      .dashboard-grid { grid-template-columns: 1fr; }
    }
  `],
})
export class HomeComponent implements OnInit {
  authService = inject(AuthService);
  private walletService = inject(WalletService);
  private marketService = inject(MarketService);
  private transactionService = inject(TransactionService);

  loading = signal(true);
  wallet = this.walletService.walletOverview;
  tickers = this.marketService.tickers;
  recentTx = signal<Transaction[]>([]);

  txIconMap: Record<string, string> = {
    send: 'arrow_upward',
    receive: 'arrow_downward',
    buy: 'shopping_cart',
    sell: 'sell',
  };

  async ngOnInit(): Promise<void> {
    const userId = this.authService.user()?.id ?? 'user-001';
    try {
      await Promise.all([
        this.walletService.getWalletOverview(userId),
        this.marketService.getTickers(),
        this.loadRecentTx(userId),
      ]);
    } finally {
      this.loading.set(false);
    }
  }

  private async loadRecentTx(userId: string): Promise<void> {
    const res = await this.transactionService.getTransactions(userId, {
      type: 'all', page: 1, pageSize: 5,
    });
    this.recentTx.set(res.items);
  }
}
