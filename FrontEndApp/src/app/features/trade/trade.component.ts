import { Component, inject, signal, effect, untracked, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { TradeService } from '../../core/services/trade.service';
import { AuthService } from '../../core/services/auth.service';
import { OrderHubService } from '../../core/services/order-hub.service';
import { MarketDataHubService } from '../../core/services/market-data-hub.service';
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
  templateUrl: './trade.component.html',
  styleUrl: './trade.component.css',
})
export class TradeComponent implements OnInit {
  private tradeService = inject(TradeService);
  private authService = inject(AuthService);
  private orderHub = inject(OrderHubService);
  private marketHub = inject(MarketDataHubService);
  private notif = inject(NotificationService);

  loading = signal(true);
  selectedPair = this.tradeService.selectedPair;
  openOrders = this.tradeService.openOrders;

  pairSearch = '';
  private searchTerm = signal('');

  // Derived from the service's pairs so live ticker pushes flow straight through to the chips.
  readonly filteredPairs = computed(() => {
    const q = this.searchTerm().toLowerCase();
    const all = this.tradeService.pairs();
    return q
      ? all.filter(p => p.symbol.toLowerCase().includes(q) || p.baseAsset.toLowerCase().includes(q))
      : all;
  });

  constructor() {
    // Fold live 24h stats into the cached pairs. Writes run untracked so updating the pairs
    // can't re-trigger this effect.
    effect(() => {
      const tickers = this.marketHub.tickers();
      untracked(() => {
        for (const ticker of Object.values(tickers)) {
          this.tradeService.applyLiveTicker(ticker);
        }
      });
    });
  }

  async ngOnInit(): Promise<void> {
    const pairs = await this.tradeService.getTradePairs();
    if (pairs.length) this.selectPair(pairs[0]);
    // Join every displayed pair's hub group so all chips tick, not just the charted one.
    for (const pair of pairs) {
      this.marketHub.subscribe(pair.symbol);
    }
    const userId = this.authService.user()?.id ?? 'user-001';
    await this.tradeService.getOpenOrders(userId);
    this.loading.set(false);
    // Live order-fill updates: keeps Open Orders + balances current without polling.
    this.orderHub.ensureConnected();
  }

  selectPair(pair: TradePair): void {
    this.tradeService.selectedPair.set(pair);
  }

  filterPairs(): void {
    this.searchTerm.set(this.pairSearch);
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
