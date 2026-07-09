import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { LivePrice } from '../models/trade.model';

@Injectable({ providedIn: 'root' })
export class MarketDataHubService {
  private readonly baseUrl = environment.apiBase.market;
  private readonly subscribedSymbols = new Set<string>();
  private connection: signalR.HubConnection | null = null;
  private connectionStart: Promise<void> | null = null;

  readonly prices = signal<Record<string, LivePrice>>({});

  constructor(private http: HttpClient) {}

  async subscribe(symbol: string): Promise<void> {
    try {
      await firstValueFrom(this.http.post(`${this.baseUrl}/Markets/subscribe/${symbol}`, {}));
      const connection = await this.ensureConnection();
      this.subscribedSymbols.add(symbol);
      await connection.invoke('Subscribe', symbol);
    } catch (err) {
      console.error('[MarketDataHubService] subscribe error:', err);
    }
  }

  async unsubscribe(symbol: string): Promise<void> {
    this.subscribedSymbols.delete(symbol);
    if (this.connection?.state !== signalR.HubConnectionState.Connected) {
      return;
    }
    try {
      await this.connection.invoke('Unsubscribe', symbol);
    } catch (err) {
      console.error('[MarketDataHubService] unsubscribe error:', err);
    }
  }

  private async ensureConnection(): Promise<signalR.HubConnection> {
    if (!this.connection) {
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(environment.marketHub, { withCredentials: false })
        .withAutomaticReconnect()
        .build();

      this.connection.on('PriceUpdated', (price: LivePrice) => {
        this.prices.update(current => ({ ...current, [price.symbol]: price }));
      });

      this.connection.onreconnected(() => {
        for (const symbol of this.subscribedSymbols) {
          this.connection?.invoke('Subscribe', symbol);
        }
      });
    }

    if (!this.connectionStart) {
      this.connectionStart = this.connection.start().catch(err => {
        this.connectionStart = null;
        throw err;
      });
    }
    await this.connectionStart;
    return this.connection;
  }
}
