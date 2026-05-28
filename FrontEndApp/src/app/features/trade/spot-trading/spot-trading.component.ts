import { Component, inject, signal, OnInit, Input } from '@angular/core';
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
import { TradePair } from '../../../core/models/trade.model';

@Component({
  selector: 'app-spot-trading',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, FormsModule,
    MatButtonModule, MatFormFieldModule, MatInputModule,
    MatTabsModule, MatSliderModule, MatSelectModule,
  ],
  template: `
    <div class="spot-trading">
      <div class="trading-header">
        <h3>Spot Trading</h3>
        <div class="order-type">
          <button [class.active]="orderType() === 'limit'" (click)="orderType.set('limit')">Limit</button>
          <button [class.active]="orderType() === 'market'" (click)="setMarket()">Market</button>
        </div>
      </div>

      <div class="buy-sell-grid">
        <!-- BUY -->
        <div class="order-form buy-form">
          <div class="form-label buy-label">Buy {{ baseAsset }}</div>
          <div class="balance-info">
            Available: <strong>{{ quoteBalance() | number:'1.2-2' }} {{ quoteAsset }}</strong>
          </div>

          @if (orderType() === 'limit') {
            <div class="input-group">
              <label>Price ({{ quoteAsset }})</label>
              <div class="input-wrapper">
                <input type="number" [(ngModel)]="buyPrice" (input)="calcBuyTotal()" step="any" />
                <span class="input-suffix">{{ quoteAsset }}</span>
              </div>
            </div>
          }

          <div class="input-group">
            <label>Amount ({{ baseAsset }})</label>
            <div class="input-wrapper">
              <input type="number" [(ngModel)]="buyAmount" (input)="calcBuyTotal()" step="any" />
              <span class="input-suffix">{{ baseAsset }}</span>
            </div>
          </div>

          <div class="pct-buttons">
            @for (pct of pcts; track pct) {
              <button (click)="setBuyPct(pct)">{{ pct }}%</button>
            }
          </div>

          <div class="input-group">
            <label>Total ({{ quoteAsset }})</label>
            <div class="input-wrapper">
              <input type="number" [(ngModel)]="buyTotal" (input)="calcBuyAmount()" step="any" />
              <span class="input-suffix">{{ quoteAsset }}</span>
            </div>
          </div>

          <div class="fee-row">
            Fee: <span>{{ (buyTotal * 0.001) | number:'1.4-4' }} {{ quoteAsset }}</span>
          </div>

          <button class="submit-btn buy-btn" (click)="placeOrder('buy')" [disabled]="buyLoading()">
            @if (buyLoading()) { <span class="btn-spinner"></span> }
            Buy {{ baseAsset }}
          </button>
        </div>

        <!-- SELL -->
        <div class="order-form sell-form">
          <div class="form-label sell-label">Sell {{ baseAsset }}</div>
          <div class="balance-info">
            Available: <strong>{{ baseBalance() | number:'1.4-6' }} {{ baseAsset }}</strong>
          </div>

          @if (orderType() === 'limit') {
            <div class="input-group">
              <label>Price ({{ quoteAsset }})</label>
              <div class="input-wrapper">
                <input type="number" [(ngModel)]="sellPrice" (input)="calcSellTotal()" step="any" />
                <span class="input-suffix">{{ quoteAsset }}</span>
              </div>
            </div>
          }

          <div class="input-group">
            <label>Amount ({{ baseAsset }})</label>
            <div class="input-wrapper">
              <input type="number" [(ngModel)]="sellAmount" (input)="calcSellTotal()" step="any" />
              <span class="input-suffix">{{ baseAsset }}</span>
            </div>
          </div>

          <div class="pct-buttons">
            @for (pct of pcts; track pct) {
              <button (click)="setSellPct(pct)">{{ pct }}%</button>
            }
          </div>

          <div class="input-group">
            <label>Total ({{ quoteAsset }})</label>
            <div class="input-wrapper">
              <input type="number" [value]="sellTotal | number:'1.2-4'" readonly />
              <span class="input-suffix">{{ quoteAsset }}</span>
            </div>
          </div>

          <div class="fee-row">
            Fee: <span>{{ (sellTotal * 0.001) | number:'1.4-4' }} {{ quoteAsset }}</span>
          </div>

          <button class="submit-btn sell-btn" (click)="placeOrder('sell')" [disabled]="sellLoading()">
            @if (sellLoading()) { <span class="btn-spinner"></span> }
            Sell {{ baseAsset }}
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .spot-trading { padding: 16px; display: flex; flex-direction: column; gap: 12px; }
    .trading-header { display: flex; align-items: center; justify-content: space-between; }
    .trading-header h3 { margin: 0; font-size: 0.9rem; font-weight: 700; color: var(--text-primary); }
    .order-type { display: flex; background: var(--bg-primary); border-radius: 6px; padding: 2px; }
    .order-type button {
      padding: 4px 12px; border: none; border-radius: 4px; cursor: pointer;
      font-size: 0.78rem; font-weight: 600; background: transparent;
      color: var(--text-secondary); transition: all 0.15s;
    }
    .order-type button.active { background: var(--accent); color: white; }
    .buy-sell-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
    .order-form { display: flex; flex-direction: column; gap: 10px; }
    .form-label { font-size: 0.82rem; font-weight: 700; }
    .buy-label { color: var(--success); }
    .sell-label { color: var(--danger); }
    .balance-info { font-size: 0.76rem; color: var(--text-secondary); }
    .balance-info strong { color: var(--text-primary); }
    .input-group { display: flex; flex-direction: column; gap: 4px; }
    .input-group label { font-size: 0.74rem; color: var(--text-secondary); }
    .input-wrapper {
      display: flex; align-items: center;
      background: var(--bg-primary); border: 1px solid var(--border-color);
      border-radius: 6px; overflow: hidden;
    }
    .input-wrapper input {
      flex: 1; padding: 8px 10px; background: transparent;
      border: none; outline: none; color: var(--text-primary);
      font-size: 0.88rem;
    }
    .input-suffix {
      padding: 0 10px; font-size: 0.76rem;
      color: var(--text-secondary); border-left: 1px solid var(--border-color);
      white-space: nowrap;
    }
    .pct-buttons { display: flex; gap: 4px; }
    .pct-buttons button {
      flex: 1; padding: 4px; border: 1px solid var(--border-color);
      background: transparent; color: var(--text-secondary);
      border-radius: 4px; cursor: pointer; font-size: 0.72rem;
      transition: all 0.15s;
    }
    .pct-buttons button:hover { border-color: var(--accent); color: var(--accent); }
    .fee-row { font-size: 0.75rem; color: var(--text-secondary); }
    .fee-row span { color: var(--text-primary); }
    .submit-btn {
      width: 100%; padding: 10px; border: none; border-radius: 8px;
      font-size: 0.88rem; font-weight: 700; cursor: pointer;
      transition: all 0.15s; display: flex; align-items: center; justify-content: center; gap: 6px;
    }
    .submit-btn:disabled { opacity: 0.6; cursor: not-allowed; }
    .buy-btn { background: var(--success); color: white; }
    .buy-btn:hover:not(:disabled) { filter: brightness(1.1); }
    .sell-btn { background: var(--danger); color: white; }
    .sell-btn:hover:not(:disabled) { filter: brightness(1.1); }
    .btn-spinner { width: 14px; height: 14px; border: 2px solid rgba(255,255,255,0.3); border-top-color: white; border-radius: 50%; animation: spin 0.8s linear infinite; }
    @keyframes spin { to { transform: rotate(360deg); } }
  `],
})
export class SpotTradingComponent implements OnInit {
  @Input() pair!: TradePair;

  private tradeService = inject(TradeService);
  private walletService = inject(WalletService);
  private authService = inject(AuthService);
  private notif = inject(NotificationService);

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
