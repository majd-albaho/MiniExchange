import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { MarketTicker, TradePair } from '../models/trade.model';
import { firstValueFrom } from 'rxjs';
import { symbolToPair, PREFERRED_SYMBOL_ORDER } from '../models/market-symbol';

interface TickerDto {
  symbol: string;
  lastPrice: number;
  priceChangePercent: number;
  highPrice: number;
  lowPrice: number;
  baseVolume: number;
  quoteVolume: number;
  eventTime: string;
}

@Injectable({ providedIn: 'root' })
export class MarketService {
  private readonly baseUrl = environment.apiBase.market;

  readonly tickers = signal<MarketTicker[]>([]);
  readonly topGainers = signal<TradePair[]>([]);
  readonly topLosers = signal<TradePair[]>([]);

  constructor(private http: HttpClient) {}

  async getTickers(): Promise<MarketTicker[]> {
    try {
      const res = await firstValueFrom(
        this.http.get<TickerDto[]>(`${this.baseUrl}/Markets/tickers`)
      );
      const mapped = res.map(t => this.mapTicker(t)).sort(this.byPreferredOrder);
      this.tickers.set(mapped);
      return mapped;
    } catch (err) {
      console.error('[MarketService] getTickers error:', err);
      const dummy: MarketTicker[] = [
        { pair: 'BTC/USDT', price: 44187.32, change24h: 1.85, high24h: 44800, low24h: 43200, volume24h: 1284500000 },
        { pair: 'ETH/USDT', price: 2440.82, change24h: 3.12, high24h: 2510, low24h: 2380, volume24h: 865200000 },
        { pair: 'SOL/USDT', price: 98.04, change24h: 5.67, high24h: 102, low24h: 92, volume24h: 124300000 },
        { pair: 'BNB/USDT', price: 312.45, change24h: -0.85, high24h: 320, low24h: 308, volume24h: 345700000 },
        { pair: 'ADA/USDT', price: 0.587, change24h: 2.10, high24h: 0.61, low24h: 0.57, volume24h: 89200000 },
        { pair: 'XRP/USDT', price: 0.622, change24h: -1.20, high24h: 0.64, low24h: 0.61, volume24h: 213400000 },
      ];
      this.tickers.set(dummy);
      return dummy;
    }
  }

  private mapTicker(dto: TickerDto): MarketTicker {
    return {
      pair: symbolToPair(dto.symbol),
      price: dto.lastPrice,
      change24h: dto.priceChangePercent,
      high24h: dto.highPrice,
      low24h: dto.lowPrice,
      volume24h: dto.quoteVolume,
    };
  }

  private byPreferredOrder = (a: MarketTicker, b: MarketTicker): number => {
    const rank = (pair: string): number => {
      const base = pair.split('/')[0];
      const i = PREFERRED_SYMBOL_ORDER.indexOf(base);
      return i === -1 ? PREFERRED_SYMBOL_ORDER.length : i;
    };
    const diff = rank(a.pair) - rank(b.pair);
    return diff !== 0 ? diff : a.pair.localeCompare(b.pair);
  };
}
