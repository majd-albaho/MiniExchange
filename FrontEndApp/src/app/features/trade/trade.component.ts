import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { TradeService } from '../../core/services/trade.service';
import { AuthService } from '../../core/services/auth.service';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';
import { TradeChartComponent } from './trade-chart/trade-chart.component';
import { OrderBookComponent } from './order-book/order-book.component';
import { SpotTradingComponent } from './spot-trading/spot-trading.component';
import { TradePair, Order } from '../../core/models/trade.model';
import { NotificationService } from '../../core/services/notification.service';

@Component({
  selector: 'app-trade',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MatButtonModule, MatIconModule,
    MatInputModule, MatFormFieldModule,
    LoadingSpinnerComponent,
    TradeChartComponent, OrderBookComponent, SpotTradingComponent,
  ],
  template: `
    <div class="trade-page">

      <!-- Pair Selector Bar -->
      <div class="pair-bar">
        <div class="search-wrapper">
          <mat-icon>search</mat-icon>
          <input placeholder="Search pairs..." [(ngModel)]="pairSearch" (input)="filterPairs()" />
        </div>
        <div class="pairs-list">
          @for (p of filteredPairs(); track p.symbol) {
            <div class="pair-chip" [class.active]="selectedPair()?.symbol === p.symbol" (click)="selectPair(p)">
              <img [src]="p.logoUrl" [alt]="p.baseAsset" class="pair-logo"
                onerror="this.src='https://via.placeholder.com/20'" />
              <span class="pair-symbol">{{ p.baseAsset }}/{{ p.quoteAsset }}</span>
              <span class="pair-price">\${{ p.lastPrice | number:'1.2-6' }}</span>
              <span class="pair-change" [class.up]="p.change24h >= 0" [class.down]="p.change24h < 0">
                {{ p.change24h >= 0 ? '+' : '' }}{{ p.change24h | number:'1.2-2' }}%
              </span>
            </div>
          }
        </div>
      </div>

      @if (loading()) {
        <app-loading-spinner message="Loading trading data..." />
      } @else if (selectedPair()) {
        <!-- Current Pair Info -->
        <div class="pair-info-bar">
          <div class="pair-name">
            <img [src]="selectedPair()!.logoUrl" class="pair-logo-lg"
              onerror="this.src='https://via.placeholder.com/32'" />
            <h2>{{ selectedPair()!.baseAsset }}/{{ selectedPair()!.quoteAsset }}</h2>
          </div>
          <div class="pair-stats">
            <div class="stat">
              <div class="stat-label">Last Price</div>
              <div class="stat-val" [class.up]="selectedPair()!.change24h >= 0" [class.down]="selectedPair()!.change24h < 0">
                \${{ selectedPair()!.lastPrice | number:'1.2-6' }}
              </div>
            </div>
            <div class="stat">
              <div class="stat-label">24h Change</div>
              <div class="stat-val" [class.up]="selectedPair()!.change24h >= 0" [class.down]="selectedPair()!.change24h < 0">
                {{ selectedPair()!.change24h >= 0 ? '+' : '' }}{{ selectedPair()!.change24h | number:'1.2-2' }}%
              </div>
            </div>
            <div class="stat">
              <div class="stat-label">24h High</div>
              <div class="stat-val">\${{ selectedPair()!.high24h | number:'1.2-2' }}</div>
            </div>
            <div class="stat">
              <div class="stat-label">24h Low</div>
              <div class="stat-val">\${{ selectedPair()!.low24h | number:'1.2-2' }}</div>
            </div>
            <div class="stat">
              <div class="stat-label">24h Volume</div>
              <div class="stat-val">\${{ formatVolume(selectedPair()!.volume24h) }}</div>
            </div>
          </div>
        </div>

        <!-- Trading Layout -->
        <div class="trading-layout">

          <!-- Chart (center) -->
          <div class="chart-panel panel">
            <app-trade-chart [pair]="selectedPair()!.symbol" />
          </div>

          <!-- Order Book (right) -->
          <div class="orderbook-panel panel">
            <div class="panel-title">Order Book</div>
            <app-order-book
              [symbol]="selectedPair()!.symbol"
              [baseAsset]="selectedPair()!.baseAsset"
              [quoteAsset]="selectedPair()!.quoteAsset"
            />
          </div>

          <!-- Spot Trading (bottom-right or below) -->
          <div class="trading-panel panel">
            <app-spot-trading [pair]="selectedPair()!" />
          </div>
        </div>

        <!-- Open Orders -->
        <div class="open-orders-panel panel">
          <div class="panel-title">Open Orders</div>
          @if (openOrders().length === 0) {
            <div class="empty-orders">No open orders</div>
          } @else {
            <div class="orders-table">
              <div class="orders-header">
                <span>Pair</span><span>Type</span><span>Side</span><span>Price</span>
                <span>Amount</span><span>Filled</span><span>Total</span><span>Date</span><span>Cancel</span>
              </div>
              @for (order of openOrders(); track order.id) {
                <div class="orders-row">
                  <span>{{ order.pair }}</span>
                  <span>{{ order.type }}</span>
                  <span [class]="order.side === 'buy' ? 'up' : 'down'">{{ order.side | titlecase }}</span>
                  <span>\${{ order.price | number:'1.2-6' }}</span>
                  <span>{{ order.amount | number:'1.4-6' }}</span>
                  <span>{{ order.filled | number:'1.4-6' }}</span>
                  <span>\${{ order.total | number:'1.2-2' }}</span>
                  <span>{{ order.createdAt | date:'HH:mm' }}</span>
                  <button mat-icon-button (click)="cancelOrder(order)">
                    <mat-icon>cancel</mat-icon>
                  </button>
                </div>
              }
            </div>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .trade-page { display: flex; flex-direction: column; gap: 12px; }
    .pair-bar {
      background: var(--bg-card); border: 1px solid var(--border-color);
      border-radius: 12px; padding: 12px 16px;
      display: flex; align-items: center; gap: 16px;
    }
    .search-wrapper {
      display: flex; align-items: center; gap: 6px;
      background: var(--bg-primary); border: 1px solid var(--border-color);
      border-radius: 8px; padding: 6px 10px; min-width: 200px;
    }
    .search-wrapper mat-icon { font-size: 18px; color: var(--text-secondary); }
    .search-wrapper input { background: none; border: none; outline: none; color: var(--text-primary); font-size: 0.85rem; width: 140px; }
    .pairs-list { display: flex; gap: 6px; overflow-x: auto; flex: 1; }
    .pair-chip {
      display: flex; align-items: center; gap: 6px;
      padding: 6px 12px; border-radius: 20px; border: 1px solid var(--border-color);
      cursor: pointer; transition: all 0.15s; white-space: nowrap; flex-shrink: 0;
      background: var(--bg-primary);
    }
    .pair-chip:hover, .pair-chip.active { border-color: var(--accent); background: var(--accent-alpha); }
    .pair-logo { width: 20px; height: 20px; border-radius: 50%; }
    .pair-symbol { font-size: 0.82rem; font-weight: 700; color: var(--text-primary); }
    .pair-price { font-size: 0.78rem; color: var(--text-secondary); }
    .pair-change { font-size: 0.78rem; font-weight: 600; }
    .up { color: var(--success); }
    .down { color: var(--danger); }
    .pair-info-bar {
      background: var(--bg-card); border: 1px solid var(--border-color);
      border-radius: 12px; padding: 14px 20px;
      display: flex; align-items: center; gap: 32px;
    }
    .pair-name { display: flex; align-items: center; gap: 10px; }
    .pair-logo-lg { width: 32px; height: 32px; border-radius: 50%; }
    .pair-name h2 { margin: 0; font-size: 1.2rem; font-weight: 700; color: var(--text-primary); }
    .pair-stats { display: flex; gap: 28px; flex: 1; }
    .stat { display: flex; flex-direction: column; gap: 2px; }
    .stat-label { font-size: 0.72rem; color: var(--text-secondary); text-transform: uppercase; letter-spacing: 0.5px; }
    .stat-val { font-size: 0.9rem; font-weight: 600; color: var(--text-primary); }
    .panel { background: var(--bg-card); border: 1px solid var(--border-color); border-radius: 12px; overflow: hidden; }
    .panel-title { padding: 12px 16px; font-size: 0.82rem; font-weight: 700; color: var(--text-secondary); text-transform: uppercase; letter-spacing: 0.5px; border-bottom: 1px solid var(--border-color); }
    .trading-layout { display: grid; grid-template-columns: 1fr 260px; grid-template-rows: auto auto; gap: 12px; }
    .chart-panel { grid-row: 1; grid-column: 1; min-height: 400px; }
    .orderbook-panel { grid-row: 1 / 3; grid-column: 2; }
    .trading-panel { grid-row: 2; grid-column: 1; }
    .open-orders-panel { }
    .empty-orders { padding: 20px; text-align: center; color: var(--text-secondary); font-size: 0.85rem; }
    .orders-table { }
    .orders-header, .orders-row {
      display: grid;
      grid-template-columns: 100px 70px 60px 100px 100px 100px 100px 60px 48px;
      padding: 8px 16px; font-size: 0.78rem; gap: 4px; align-items: center;
    }
    .orders-header { font-weight: 700; color: var(--text-secondary); text-transform: uppercase; font-size: 0.72px; border-bottom: 1px solid var(--border-color); }
    .orders-row { border-bottom: 1px solid var(--border-color); color: var(--text-primary); }
    .orders-row:last-child { border-bottom: none; }
    .orders-row button { color: var(--danger) !important; width: 32px; height: 32px; }
    @media (max-width: 1200px) {
      .trading-layout { grid-template-columns: 1fr; grid-template-rows: auto; }
      .orderbook-panel { grid-row: auto; grid-column: auto; }
    }
  `],
})
export class TradeComponent implements OnInit {
  private tradeService = inject(TradeService);
  private authService = inject(AuthService);
  private notif = inject(NotificationService);

  loading = signal(true);
  selectedPair = this.tradeService.selectedPair;
  openOrders = this.tradeService.openOrders;

  pairSearch = '';
  filteredPairs = signal<TradePair[]>([]);

  async ngOnInit(): Promise<void> {
    const pairs = await this.tradeService.getTradePairs();
    this.filteredPairs.set(pairs);
    if (pairs.length) this.selectPair(pairs[0]);
    const userId = this.authService.user()?.id ?? 'user-001';
    await this.tradeService.getOpenOrders(userId);
    this.loading.set(false);
  }

  selectPair(pair: TradePair): void {
    this.tradeService.selectedPair.set(pair);
  }

  filterPairs(): void {
    const q = this.pairSearch.toLowerCase();
    const all = this.tradeService.pairs();
    this.filteredPairs.set(
      q ? all.filter(p => p.symbol.toLowerCase().includes(q) || p.baseAsset.toLowerCase().includes(q)) : all
    );
  }

  async cancelOrder(order: Order): Promise<void> {
    await this.tradeService.cancelOrder(order.id);
    this.notif.success(`Order ${order.id} cancelled`);
  }

  formatVolume(v: number): string {
    if (v >= 1_000_000_000) return (v / 1_000_000_000).toFixed(2) + 'B';
    if (v >= 1_000_000) return (v / 1_000_000).toFixed(2) + 'M';
    return v.toFixed(0);
  }
}
