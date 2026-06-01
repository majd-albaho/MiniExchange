import { Component, inject, signal, OnInit, Input, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TradeService } from '../../../core/services/trade.service';
import { OrderBook, OrderBookEntry } from '../../../core/models/trade.model';

@Component({
  selector: 'app-order-book',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './order-book.component.html',
  styleUrl: './order-book.component.css',
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
