import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { WalletService } from '../../core/services/wallet.service';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';
import { WalletAsset } from '../../core/models/wallet.model';
import { SendDialogComponent } from './send-dialog/send-dialog.component';
import { ReceiveDialogComponent } from './receive-dialog/receive-dialog.component';

@Component({
  selector: 'app-wallet',
  standalone: true,
  imports: [
    CommonModule, MatButtonModule, MatIconModule, MatTooltipModule,
    RouterLink, PageHeaderComponent, LoadingSpinnerComponent,
  ],
  template: `
    <div class="wallet-page">
      <app-page-header title="My Wallet" subtitle="Manage your crypto assets">
        <button mat-stroked-button routerLink="/transactions">
          <mat-icon>receipt_long</mat-icon> Transaction History
        </button>
      </app-page-header>

      @if (loading()) {
        <app-loading-spinner message="Loading wallet..." />
      } @else {
        <!-- Total Balance Card -->
        <div class="balance-card">
          <div class="balance-left">
            <div class="balance-label">Total Portfolio Value</div>
            <div class="balance-amount">
              \${{ walletService.walletOverview()?.totalBalanceUSDT | number:'1.2-2' }}
              <span class="balance-currency">USDT</span>
            </div>
            <div class="balance-change"
              [class.up]="(walletService.walletOverview()?.totalChange24h ?? 0) >= 0"
              [class.down]="(walletService.walletOverview()?.totalChange24h ?? 0) < 0">
              {{ (walletService.walletOverview()?.totalChange24h ?? 0) >= 0 ? '▲' : '▼' }}
              {{ walletService.walletOverview()?.totalChange24h | number:'1.2-2' }}% (24h)
            </div>
          </div>
          <div class="balance-actions">
            <button mat-raised-button color="primary" (click)="openSendDialog()">
              <mat-icon>arrow_upward</mat-icon> Send
            </button>
            <button mat-raised-button class="receive-btn" (click)="openReceiveDialog()">
              <mat-icon>arrow_downward</mat-icon> Receive
            </button>
          </div>
        </div>

        <!-- Asset List -->
        <div class="assets-section">
          <div class="section-header">
            <h3>Assets</h3>
            <div class="search-bar">
              <mat-icon>search</mat-icon>
              <input
                placeholder="Search assets..."
                [(value)]="searchQuery"
                (input)="onSearch($event)"
              />
            </div>
          </div>

          <div class="assets-table">
            <div class="table-header">
              <span>Asset</span>
              <span>Price</span>
              <span>24h Change</span>
              <span>Balance</span>
              <span>Value (USDT)</span>
              <span>Actions</span>
            </div>

            @for (asset of filteredAssets(); track asset.id) {
              <div class="table-row">
                <div class="asset-cell">
                  <img [src]="asset.logoUrl" [alt]="asset.symbol" class="asset-logo"
                    onerror="this.src='https://via.placeholder.com/32'" />
                  <div>
                    <div class="asset-symbol">{{ asset.symbol }}</div>
                    <div class="asset-name">{{ asset.name }}</div>
                  </div>
                </div>
                <div class="price-cell">\${{ asset.price | number:'1.2-6' }}</div>
                <div class="change-cell" [class.up]="asset.change24h >= 0" [class.down]="asset.change24h < 0">
                  {{ asset.change24h >= 0 ? '+' : '' }}{{ asset.change24h | number:'1.2-2' }}%
                </div>
                <div class="balance-cell">
                  <div>{{ asset.balance | number:'1.4-8' }} {{ asset.symbol }}</div>
                </div>
                <div class="value-cell">\${{ asset.balanceUSDT | number:'1.2-2' }}</div>
                <div class="actions-cell">
                  <button mat-icon-button matTooltip="Send" (click)="openSendDialog(asset)">
                    <mat-icon>arrow_upward</mat-icon>
                  </button>
                  <button mat-icon-button matTooltip="Receive" (click)="openReceiveDialog(asset)">
                    <mat-icon>arrow_downward</mat-icon>
                  </button>
                  <button mat-icon-button matTooltip="Trade" routerLink="/trade">
                    <mat-icon>candlestick_chart</mat-icon>
                  </button>
                </div>
              </div>
            }

            @if (filteredAssets().length === 0) {
              <div class="empty-state">
                <mat-icon>search_off</mat-icon>
                <p>No assets found</p>
              </div>
            }
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .wallet-page { display: flex; flex-direction: column; gap: 20px; }
    .balance-card {
      background: linear-gradient(135deg, #1a2a4a 0%, #0d1117 100%);
      border: 1px solid var(--border-color);
      border-radius: 16px; padding: 28px 32px;
      display: flex; align-items: center; justify-content: space-between;
    }
    .balance-label { font-size: 0.82rem; color: rgba(255,255,255,0.6); margin-bottom: 8px; text-transform: uppercase; letter-spacing: 0.5px; }
    .balance-amount { font-size: 2.2rem; font-weight: 800; color: white; display: flex; align-items: baseline; gap: 8px; }
    .balance-currency { font-size: 1rem; color: rgba(255,255,255,0.6); font-weight: 400; }
    .balance-change { font-size: 0.9rem; font-weight: 600; margin-top: 6px; }
    .up { color: var(--success) !important; }
    .down { color: var(--danger) !important; }
    .balance-actions { display: flex; gap: 12px; }
    .receive-btn { background: rgba(255,255,255,0.1) !important; color: white !important; }
    .assets-section { background: var(--bg-card); border: 1px solid var(--border-color); border-radius: 12px; overflow: hidden; }
    .section-header {
      display: flex; align-items: center; justify-content: space-between;
      padding: 16px 20px; border-bottom: 1px solid var(--border-color);
    }
    .section-header h3 { margin: 0; font-size: 1rem; font-weight: 700; color: var(--text-primary); }
    .search-bar {
      display: flex; align-items: center; gap: 8px;
      background: var(--bg-primary); border: 1px solid var(--border-color);
      border-radius: 8px; padding: 6px 12px;
    }
    .search-bar mat-icon { font-size: 18px; color: var(--text-secondary); }
    .search-bar input { background: none; border: none; outline: none; color: var(--text-primary); font-size: 0.88rem; width: 200px; }
    .table-header {
      display: grid; grid-template-columns: 2fr 1fr 1fr 1.5fr 1fr 1fr;
      padding: 10px 20px; font-size: 0.75rem; font-weight: 700;
      color: var(--text-secondary); text-transform: uppercase; letter-spacing: 0.5px;
      border-bottom: 1px solid var(--border-color);
    }
    .table-row {
      display: grid; grid-template-columns: 2fr 1fr 1fr 1.5fr 1fr 1fr;
      padding: 14px 20px; align-items: center;
      border-bottom: 1px solid var(--border-color);
      transition: background 0.15s;
    }
    .table-row:hover { background: var(--bg-primary); }
    .table-row:last-child { border-bottom: none; }
    .asset-cell { display: flex; align-items: center; gap: 12px; }
    .asset-logo { width: 36px; height: 36px; border-radius: 50%; object-fit: contain; }
    .asset-symbol { font-size: 0.9rem; font-weight: 700; color: var(--text-primary); }
    .asset-name { font-size: 0.76rem; color: var(--text-secondary); }
    .price-cell { font-size: 0.9rem; color: var(--text-primary); font-weight: 500; }
    .change-cell { font-size: 0.88rem; font-weight: 600; }
    .balance-cell { font-size: 0.88rem; color: var(--text-primary); }
    .value-cell { font-size: 0.9rem; font-weight: 600; color: var(--text-primary); }
    .actions-cell { display: flex; gap: 4px; }
    .actions-cell button { color: var(--text-secondary) !important; }
    .actions-cell button:hover { color: var(--accent) !important; }
    .empty-state { display: flex; flex-direction: column; align-items: center; padding: 48px; gap: 12px; color: var(--text-secondary); }
    .empty-state mat-icon { font-size: 40px; width: 40px; height: 40px; opacity: 0.4; }
  `],
})
export class WalletComponent implements OnInit {
  private authService = inject(AuthService);
  walletService = inject(WalletService);
  private dialog = inject(MatDialog);

  loading = signal(true);
  searchQuery = '';
  filteredAssets = signal<WalletAsset[]>([]);

  async ngOnInit(): Promise<void> {
    const userId = this.authService.user()?.id ?? 'user-001';
    await this.walletService.getWalletOverview(userId);
    this.filteredAssets.set(this.walletService.walletOverview()?.assets ?? []);
    this.loading.set(false);
  }

  onSearch(event: Event): void {
    const query = (event.target as HTMLInputElement).value.toLowerCase();
    const all = this.walletService.walletOverview()?.assets ?? [];
    this.filteredAssets.set(
      all.filter(a => a.symbol.toLowerCase().includes(query) || a.name.toLowerCase().includes(query))
    );
  }

  openSendDialog(asset?: WalletAsset): void {
    const allAssets = this.walletService.walletOverview()?.assets ?? [];
    const target = asset ?? allAssets[0];
    if (!target) return;
    this.dialog.open(SendDialogComponent, {
      data: { asset: target, allAssets },
      panelClass: 'dark-dialog',
    });
  }

  openReceiveDialog(asset?: WalletAsset): void {
    const allAssets = this.walletService.walletOverview()?.assets ?? [];
    const target = asset ?? allAssets[0];
    if (!target) return;
    const userId = this.authService.user()?.id ?? 'user-001';
    this.dialog.open(ReceiveDialogComponent, {
      data: { asset: target, allAssets, userId },
      panelClass: 'dark-dialog',
    });
  }
}
