import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { MarketTicker, TradePair } from '../models/trade.model';
import { firstValueFrom } from 'rxjs';

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
        this.http.get<MarketTicker[]>(`${this.baseUrl}/tickers`)
      );
      this.tickers.set(res);
      return res;
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
}
