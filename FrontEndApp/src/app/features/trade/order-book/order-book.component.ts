import { Component, inject, signal, OnInit, Input, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TradeService } from '../../../core/services/trade.service';
import { OrderBook, OrderBookEntry } from '../../../core/models/trade.model';

@Component({
  selector: 'app-order-book',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="order-book">
      <div class="ob-header">
        <span>Price ({{ quoteAsset }})</span>
        <span>Amount ({{ baseAsset }})</span>
        <span>Total</span>
      </div>

      <!-- Asks (Sell side) - displayed in reverse so highest ask is furthest from mid -->
      <div class="asks">
        @for (ask of asks(); track ask.price) {
          <div class="ob-row ask">
            <div class="depth-bar ask-bar" [style.width.%]="getDepthPct(ask.total, maxAsk())"></div>
            <span class="price ask-price">{{ ask.price | number:'1.2-6' }}</span>
            <span class="amount">{{ ask.amount | number:'1.4-6' }}</span>
            <span class="total">{{ ask.total | number:'1.2-2' }}</span>
          </div>
        }
      </div>

      <!-- Spread -->
      <div class="spread">
        <span class="spread-price">{{ midPrice() | number:'1.2-6' }}</span>
        <span class="spread-label">Spread: {{ spread() | number:'1.2-4' }}</span>
      </div>

      <!-- Bids (Buy side) -->
      <div class="bids">
        @for (bid of bids(); track bid.price) {
          <div class="ob-row bid">
            <div class="depth-bar bid-bar" [style.width.%]="getDepthPct(bid.total, maxBid())"></div>
            <span class="price bid-price">{{ bid.price | number:'1.2-6' }}</span>
            <span class="amount">{{ bid.amount | number:'1.4-6' }}</span>
            <span class="total">{{ bid.total | number:'1.2-2' }}</span>
          </div>
        }
      </div>
    </div>
  `,
  styles: [`
    .order-book { display: flex; flex-direction: column; height: 100%; font-size: 0.78rem; }
    .ob-header {
      display: grid; grid-template-columns: 1fr 1fr 1fr;
      padding: 8px 12px; font-size: 0.72rem; font-weight: 700;
      color: var(--text-secondary); text-transform: uppercase; letter-spacing: 0.5px;
      border-bottom: 1px solid var(--border-color);
    }
    .asks, .bids { display: flex; flex-direction: column; gap: 1px; }
    .asks { flex-direction: column-reverse; }
    .ob-row {
      display: grid; grid-template-columns: 1fr 1fr 1fr;
      padding: 3px 12px; position: relative; cursor: pointer;
      transition: background 0.1s;
    }
    .ob-row:hover { background: rgba(255,255,255,0.05); }
    .depth-bar {
      position: absolute; inset: 0; right: auto; height: 100%;
      opacity: 0.15; transition: width 0.3s;
    }
    .ask-bar { background: var(--danger); }
    .bid-bar { background: var(--success); }
    .price { font-weight: 600; position: relative; z-index: 1; }
    .ask-price { color: var(--danger); }
    .bid-price { color: var(--success); }
    .amount, .total { position: relative; z-index: 1; color: var(--text-primary); }
    .spread {
      display: flex; align-items: center; justify-content: space-between;
      padding: 8px 12px; background: var(--bg-primary);
      border-top: 1px solid var(--border-color);
      border-bottom: 1px solid var(--border-color);
    }
    .spread-price { font-size: 0.95rem; font-weight: 700; color: var(--accent); }
    .spread-label { font-size: 0.72rem; color: var(--text-secondary); }
  `],
})
export class OrderBookComponent implements OnInit, OnChanges {
  @Input() symbol = 'BTCUSDT';
  @Input() baseAsset = 'BTC';
  @Input() quoteAsset = 'USDT';

  private tradeService = inject(TradeService);

  asks = signal<OrderBookEntry[]>([]);
  bids = signal<OrderBookEntry[]>([]);
  maxAsk = signal(0);
  maxBid = signal(0);
  midPrice = signal(0);
  spread = signal(0);

  ngOnInit(): void { this.loadOrderBook(); }
  ngOnChanges(): void { this.loadOrderBook(); }

  async loadOrderBook(): Promise<void> {
    const ob = await this.tradeService.getOrderBook(this.symbol);
    this.asks.set(ob.asks.slice(0, 12));
    this.bids.set(ob.bids.slice(0, 12));
    this.maxAsk.set(Math.max(...ob.asks.map(a => a.total), 1));
    this.maxBid.set(Math.max(...ob.bids.map(b => b.total), 1));
    const bestAsk = ob.asks[0]?.price ?? 0;
    const bestBid = ob.bids[0]?.price ?? 0;
    this.midPrice.set((bestAsk + bestBid) / 2);
    this.spread.set(bestAsk - bestBid);
  }

  getDepthPct(total: number, max: number): number {
    return max > 0 ? (total / max) * 100 : 0;
  }
}
