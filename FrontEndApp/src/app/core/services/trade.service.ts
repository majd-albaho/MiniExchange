import { Injectable, signal, inject } from '@angular/core';
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
import { AuthService } from './auth.service';

interface TradingPairDto {
  symbol: string;
  baseAsset: string;
  quoteAsset: string;
  minOrderQuantity: number;
  minOrderValue: number;
  pricePrecision: number;
  quantityPrecision: number;
  isActive: boolean;
}

interface LatestPriceDto {
  symbol: string;
  lastPrice: number;
  bidPrice: number;
  askPrice: number;
  eventTime: string;
}

const ASSET_LOGOS: Record<string, string> = {
  BTC: 'https://cryptologos.cc/logos/bitcoin-btc-logo.png',
  ETH: 'https://cryptologos.cc/logos/ethereum-eth-logo.png',
  SOL: 'https://cryptologos.cc/logos/solana-sol-logo.png',
  BNB: 'https://cryptologos.cc/logos/bnb-bnb-logo.png',
  ADA: 'https://cryptologos.cc/logos/cardano-ada-logo.png',
  XRP: 'https://cryptologos.cc/logos/xrp-xrp-logo.png',
  USDT: 'https://cryptologos.cc/logos/tether-usdt-logo.png',
};

interface OrderResponseDto {
  id: string;
  userId: string;
  pairSymbol: string;
  side: 'Buy' | 'Sell';
  type: 'Market' | 'Limit';
  price: number;
  quantity: number;
  filledQuantity: number;
  status: 'Pending' | 'Filled' | 'PartiallyFilled' | 'Canceled' | 'Rejected';
  createdDate: string;
  modifiedDate: string;
}

@Injectable({ providedIn: 'root' })
export class TradeService {
  private readonly tradeUrl = environment.apiBase.trade;
  private readonly pairsUrl = environment.apiBase.pairs;
  private readonly marketUrl = environment.apiBase.market;
  private readonly authService = inject(AuthService);

  readonly pairs = signal<TradePair[]>([]);
  readonly selectedPair = signal<TradePair | null>(null);
  readonly orderBook = signal<OrderBook | null>(null);
  readonly openOrders = signal<Order[]>([]);

  constructor(private http: HttpClient) {}

  private mapOrderResponse(dto: OrderResponseDto): Order {
    const statusMap: Record<OrderResponseDto['status'], Order['status']> = {
      Pending: 'open',
      Filled: 'filled',
      PartiallyFilled: 'partial',
      Canceled: 'cancelled',
      Rejected: 'cancelled',
    };
    return {
      id: dto.id,
      pair: dto.pairSymbol,
      side: dto.side.toLowerCase() as 'buy' | 'sell',
      type: dto.type.toLowerCase() as 'market' | 'limit',
      status: statusMap[dto.status],
      amount: dto.quantity,
      filled: dto.filledQuantity,
      price: dto.price,
      avgPrice: dto.price,
      total: dto.quantity * dto.price,
      fee: 0,
      createdAt: dto.createdDate,
    };
  }

  async getTradePairs(): Promise<TradePair[]> {
    try {
      const catalog = await firstValueFrom(
        this.http.get<TradingPairDto[]>(`${this.pairsUrl}/trading-pairs`)
      );
      const active = catalog.filter(p => p.isActive);
      const res = await Promise.all(active.map(p => this.toTradePair(p)));
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

  /**
   * MarketDataService only caches prices for subscribed symbols, so subscribe first;
   * the first load may still show 0 until Binance delivers a tick, after which the
   * SignalR hub keeps the price fresh. 24h stats aren't served by the backend yet.
   */
  private async toTradePair(dto: TradingPairDto): Promise<TradePair> {
    let lastPrice = 0;
    try {
      await firstValueFrom(
        this.http.post(`${this.marketUrl}/Markets/subscribe/${dto.symbol}`, {})
      );
      const price = await firstValueFrom(
        this.http.get<LatestPriceDto>(`${this.marketUrl}/Markets/price/${dto.symbol}`)
      );
      lastPrice = price.lastPrice;
    } catch {
      // No cached price yet — leave 0 and let the live hub fill it in.
    }
    return {
      symbol: dto.symbol,
      baseAsset: dto.baseAsset,
      quoteAsset: dto.quoteAsset,
      lastPrice,
      change24h: 0,
      high24h: 0,
      low24h: 0,
      volume24h: 0,
      logoUrl: ASSET_LOGOS[dto.baseAsset] ?? '',
    };
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
        this.http.get<Candle[]>(`${this.marketUrl}/Markets/candles/${symbol}`, {
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
      const user = this.authService.user();
      const backendRequest = {
        userId: user?.id,
        pairSymbol: request.pair,
        side: request.side === 'buy' ? 'Buy' : 'Sell',
        type: request.type === 'market' ? 'Market' : 'Limit',
        price: request.type === 'limit' ? (request.price ?? 0) : 0,
        quantity: request.amount,
        createdBy: user?.email,
      };
      const res = await firstValueFrom(
        this.http.post<OrderResponseDto>(`${this.tradeUrl}/orders`, backendRequest)
      );
      return this.mapOrderResponse(res);
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
        this.http.get<OrderResponseDto[]>(`${this.tradeUrl}/orders/user/${userId}/open`)
      );
      const orders = res.map(dto => this.mapOrderResponse(dto));
      this.openOrders.set(orders);
      return orders;
    } catch (err) {
      console.error('[TradeService] getOpenOrders error:', err);
      this.openOrders.set([]);
      return [];
    }
  }

  async cancelOrder(orderId: string): Promise<void> {
    try {
      const deletedBy = this.authService.user()?.email ?? '';
      await firstValueFrom(
        this.http.delete(`${this.tradeUrl}/orders/${orderId}`, { params: { deletedBy } })
      );
      this.openOrders.update(orders => orders.filter(o => o.id !== orderId));
    } catch (err) {
      console.error('[TradeService] cancelOrder error:', err);
      this.openOrders.update(orders => orders.filter(o => o.id !== orderId));
    }
  }
}
