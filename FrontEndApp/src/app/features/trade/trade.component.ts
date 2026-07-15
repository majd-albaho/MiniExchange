import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { TradeService } from '../../core/services/trade.service';
import { AuthService } from '../../core/services/auth.service';
import { OrderHubService } from '../../core/services/order-hub.service';
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
    // Live order-fill updates: keeps Open Orders + balances current without polling.
    this.orderHub.ensureConnected();
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
