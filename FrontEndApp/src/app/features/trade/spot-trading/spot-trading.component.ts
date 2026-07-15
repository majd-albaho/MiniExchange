import { Component, inject, signal, effect, OnInit, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatTabsModule } from '@angular/material/tabs';
import { MatSliderModule } from '@angular/material/slider';
import { MatSelectModule } from '@angular/material/select';
import { TradeService } from '../../../core/services/trade.service';
import { NotificationService } from '../../../core/services/notification.service';
import { WalletService } from '../../../core/services/wallet.service';
import { AuthService } from '../../../core/services/auth.service';
import { OrderHubService } from '../../../core/services/order-hub.service';
import { TradePair } from '../../../core/models/trade.model';

@Component({
  selector: 'app-spot-trading',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, FormsModule,
    MatButtonModule, MatFormFieldModule, MatInputModule,
    MatTabsModule, MatSliderModule, MatSelectModule,
  ],
  templateUrl: './spot-trading.component.html',
  styleUrl: './spot-trading.component.css',
})
export class SpotTradingComponent implements OnInit {
  @Input() pair!: TradePair;

  private tradeService = inject(TradeService);
  private walletService = inject(WalletService);
  private authService = inject(AuthService);
  private orderHub = inject(OrderHubService);
  private notif = inject(NotificationService);

  constructor() {
    // Reload available balances whenever one of the user's orders fills.
    effect(() => {
      if (this.orderHub.lastUpdate()) {
        this.loadBalances();
      }
    });
  }

  orderType = signal<'limit' | 'market'>('limit');
  buyLoading = signal(false);
  sellLoading = signal(false);
  quoteBalance = signal(0);
  baseBalance = signal(0);

  buyPrice = 0;
  buyAmount = 0;
  buyTotal = 0;
  sellPrice = 0;
  sellAmount = 0;
  sellTotal = 0;

  pcts = [25, 50, 75, 100];

  get baseAsset(): string { return this.pair?.baseAsset ?? 'BTC'; }
  get quoteAsset(): string { return this.pair?.quoteAsset ?? 'USDT'; }

  ngOnInit(): void {
    if (this.pair) {
      this.buyPrice = this.pair.lastPrice;
      this.sellPrice = this.pair.lastPrice;
    }
    this.loadBalances();
  }

  private async loadBalances(): Promise<void> {
    const userId = this.authService.user()?.id ?? 'user-001';
    await this.walletService.getWalletOverview(userId);
    const assets = this.walletService.walletOverview()?.assets ?? [];
    const base = assets.find(a => a.symbol === this.baseAsset);
    const quote = assets.find(a => a.symbol === this.quoteAsset);
    this.baseBalance.set(base?.balance ?? 0);
    this.quoteBalance.set(quote?.balance ?? 0);
  }

  setMarket(): void {
    this.orderType.set('market');
    this.buyPrice = this.pair?.lastPrice ?? 0;
    this.sellPrice = this.pair?.lastPrice ?? 0;
    this.calcBuyTotal(); this.calcSellTotal();
  }

  calcBuyTotal(): void {
    this.buyTotal = this.buyAmount * (this.orderType() === 'limit' ? this.buyPrice : this.pair.lastPrice);
  }

  calcBuyAmount(): void {
    const price = this.orderType() === 'limit' ? this.buyPrice : this.pair.lastPrice;
    this.buyAmount = price > 0 ? this.buyTotal / price : 0;
  }

  calcSellTotal(): void {
    this.sellTotal = this.sellAmount * (this.orderType() === 'limit' ? this.sellPrice : this.pair.lastPrice);
  }

  setBuyPct(pct: number): void {
    const price = this.orderType() === 'limit' ? this.buyPrice : this.pair.lastPrice;
    this.buyTotal = (this.quoteBalance() * pct) / 100;
    this.buyAmount = price > 0 ? this.buyTotal / price : 0;
  }

  setSellPct(pct: number): void {
    this.sellAmount = (this.baseBalance() * pct) / 100;
    this.calcSellTotal();
  }

  async placeOrder(side: 'buy' | 'sell'): Promise<void> {
    const amount = side === 'buy' ? this.buyAmount : this.sellAmount;
    const price = side === 'buy' ? this.buyPrice : this.sellPrice;
    if (!amount || amount <= 0) {
      this.notif.warning('Enter a valid amount');
      return;
    }
    const loadSig = side === 'buy' ? this.buyLoading : this.sellLoading;
    loadSig.set(true);
    try {
      const order = await this.tradeService.placeOrder({
        pair: this.pair.symbol,
        side,
        type: this.orderType(),
        amount,
        price: this.orderType() === 'limit' ? price : undefined,
      });
      this.notif.success(`${side === 'buy' ? '✅ Buy' : '🔴 Sell'} order placed! ID: ${order.id}`);
      if (side === 'buy') { this.buyAmount = 0; this.buyTotal = 0; }
      else { this.sellAmount = 0; this.sellTotal = 0; }
      await this.loadBalances();
    } catch {
      this.notif.error('Failed to place order');
    } finally {
      loadSig.set(false);
    }
  }
}
