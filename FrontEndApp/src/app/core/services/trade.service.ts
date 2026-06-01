import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import {
  TradePair,
  OrderBook,
  PlaceOrderRequest,
  Order,
  Candle,
  MarketTicker,
} from '../models/trade.model';
import { firstValueFrom } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class TradeService {
  private readonly tradeUrl = environment.apiBase.trade;
  private readonly marketUrl = environment.apiBase.market;

  readonly pairs = signal<TradePair[]>([]);
  readonly selectedPair = signal<TradePair | null>(null);
  readonly orderBook = signal<OrderBook | null>(null);
  readonly openOrders = signal<Order[]>([]);

  constructor(private http: HttpClient) {}

  async getTradePairs(): Promise<TradePair[]> {
    try {
      const res = await firstValueFrom(
        this.http.get<TradePair[]>(`${this.marketUrl}/pairs`)
      );
      this.pairs.set(res);
      return res;
    } catch (err) {
      console.error('[TradeService] getTradePairs error:', err);
      const dummy: TradePair[] = [
        { symbol: 'BTCUSDT', baseAsset: 'BTC', quoteAsset: 'USDT', lastPrice: 44187.32, change24h: 1.85, high24h: 44800, low24h: 43200, volume24h: 1284500000, logoUrl: 'https://cryptologos.cc/logos/bitcoin-btc-logo.png' },
        { symbol: 'ETHUSDT', baseAsset: 'ETH', quoteAsset: 'USDT', lastPrice: 2440.82, change24h: 3.12, high24h: 2510, low24h: 2380, volume24h: 865200000, logoUrl: 'https://cryptologos.cc/logos/ethereum-eth-logo.png' },
        { symbol: 'SOLUSDT', baseAsset: 'SOL', quoteAsset: 'USDT', lastPrice: 98.04, change24h: 5.67, high24h: 102, low24h: 92, volume24h: 124300000, logoUrl: 'https://cryptologos.cc/logos/solana-sol-logo.png' },
        { symbol: 'BNBUSDT', baseAsset: 'BNB', quoteAsset: 'USDT', lastPrice: 312.45, change24h: -0.85, high24h: 320, low24h: 308, volume24h: 345700000, logoUrl: 'https://cryptologos.cc/logos/bnb-bnb-logo.png' },
        { symbol: 'ADAUSDT', baseAsset: 'ADA', quoteAsset: 'USDT', lastPrice: 0.587, change24h: 2.10, high24h: 0.61, low24h: 0.57, volume24h: 89200000, logoUrl: 'https://cryptologos.cc/logos/cardano-ada-logo.png' },
        { symbol: 'XRPUSDT', baseAsset: 'XRP', quoteAsset: 'USDT', lastPrice: 0.622, change24h: -1.20, high24h: 0.64, low24h: 0.61, volume24h: 213400000, logoUrl: 'https://cryptologos.cc/logos/xrp-xrp-logo.png' },
      ];
      this.pairs.set(dummy);
      return dummy;
    }
  }

  async getOrderBook(symbol: string): Promise<OrderBook> {
    try {
      const res = await firstValueFrom(
        this.http.get<OrderBook>(`${this.marketUrl}/orderbook/${symbol}`)
      );
      this.orderBook.set(res);
      return res;
    } catch (err) {
      console.error('[TradeService] getOrderBook error:', err);
      const basePrice = this.selectedPair()?.lastPrice ?? 44000;
      const asks: OrderBook['asks'] = [];
      const bids: OrderBook['bids'] = [];
      for (let i = 0; i < 12; i++) {
        const askPrice = +(basePrice + (i + 1) * 5).toFixed(2);
        const askAmt = +(Math.random() * 0.5).toFixed(4);
        asks.push({ price: askPrice, amount: askAmt, total: +(askPrice * askAmt).toFixed(2) });

        const bidPrice = +(basePrice - (i + 1) * 5).toFixed(2);
        const bidAmt = +(Math.random() * 0.5).toFixed(4);
        bids.push({ price: bidPrice, amount: bidAmt, total: +(bidPrice * bidAmt).toFixed(2) });
      }
      const dummy: OrderBook = { pair: symbol, asks, bids, lastUpdateTime: new Date().toISOString() };
      this.orderBook.set(dummy);
      return dummy;
    }
  }

  async getCandles(symbol: string, interval: string): Promise<Candle[]> {
    try {
      return await firstValueFrom(
        this.http.get<Candle[]>(`${this.marketUrl}/candles/${symbol}`, {
          params: { interval },
        })
      );
    } catch (err) {
      console.error('[TradeService] getCandles error:', err);
      const candles: Candle[] = [];
      const base = this.selectedPair()?.lastPrice ?? 44000;
      let price = base - 500;
      const now = Math.floor(Date.now() / 1000);
      for (let i = 99; i >= 0; i--) {
        const open = price;
        const change = (Math.random() - 0.48) * 200;
        const close = +(open + change).toFixed(2);
        const high = +(Math.max(open, close) + Math.random() * 100).toFixed(2);
        const low = +(Math.min(open, close) - Math.random() * 100).toFixed(2);
        candles.push({
          time: now - i * 3600,
          open, high, low, close,
          volume: +(Math.random() * 1000).toFixed(2),
        });
        price = close;
      }
      return candles;
    }
  }

  async placeOrder(request: PlaceOrderRequest): Promise<Order> {
    try {
      return await firstValueFrom(
        this.http.post<Order>(`${this.tradeUrl}/orders`, request)
      );
    } catch (err) {
      console.error('[TradeService] placeOrder error:', err);
      const dummy: Order = {
        id: 'order-' + Date.now(),
        pair: request.pair,
        side: request.side,
        type: request.type,
        status: request.type === 'market' ? 'filled' : 'open',
        amount: request.amount,
        filled: request.type === 'market' ? request.amount : 0,
        price: request.price ?? this.selectedPair()?.lastPrice ?? 0,
        avgPrice: request.price ?? this.selectedPair()?.lastPrice ?? 0,
        total: request.amount * (request.price ?? this.selectedPair()?.lastPrice ?? 0),
        fee: +(request.amount * (request.price ?? 0) * 0.001).toFixed(4),
        createdAt: new Date().toISOString(),
      };
      this.openOrders.update(orders => [dummy, ...orders]);
      return dummy;
    }
  }

  async getOpenOrders(userId: string): Promise<Order[]> {
    try {
      const res = await firstValueFrom(
        this.http.get<Order[]>(`${this.tradeUrl}/orders/${userId}/open`)
      );
      this.openOrders.set(res);
      return res;
    } catch (err) {
      console.error('[TradeService] getOpenOrders error:', err);
      this.openOrders.set([]);
      return [];
    }
  }

  async cancelOrder(orderId: string): Promise<void> {
    try {
      await firstValueFrom(
        this.http.delete(`${this.tradeUrl}/orders/${orderId}`)
      );
      this.openOrders.update(orders => orders.filter(o => o.id !== orderId));
    } catch (err) {
      console.error('[TradeService] cancelOrder error:', err);
      this.openOrders.update(orders => orders.filter(o => o.id !== orderId));
    }
  }
}
