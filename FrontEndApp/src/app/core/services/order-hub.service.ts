import { Injectable, inject, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';
import { TradeService } from './trade.service';
import { WalletService } from './wallet.service';
import { NotificationService } from './notification.service';
import { symbolToBaseAsset } from '../models/market-symbol';

export interface OrderUpdate {
  orderId: string;
  pairSymbol: string;
  side: 'Buy' | 'Sell';
  type: 'Market' | 'Limit';
  status: 'Pending' | 'PartiallyFilled' | 'Filled' | 'Canceled' | 'Rejected';
  quantity: number;
  filledQuantity: number;
  price: number;
  lastFillQuantity: number;
  lastFillPrice: number;
}

/**
 * Live order/balance updates pushed by TradingService when one of the user's orders fills.
 * On each update it refreshes the shared open-orders and wallet signals (so any component
 * reading them updates automatically) and raises a toast. Connection is idempotent.
 */
@Injectable({ providedIn: 'root' })
export class OrderHubService {
  private readonly authService = inject(AuthService);
  private readonly tradeService = inject(TradeService);
  private readonly walletService = inject(WalletService);
  private readonly notif = inject(NotificationService);

  private connection: signalR.HubConnection | null = null;
  private connectionStart: Promise<void> | null = null;

  /** Bumps on every received update so components can react (e.g. reload local balances). */
  readonly lastUpdate = signal<OrderUpdate | null>(null);

  async ensureConnected(): Promise<void> {
    const token = this.authService.getToken();
    if (!token) {
      return;
    }

    if (!this.connection) {
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(environment.orderHub, {
          accessTokenFactory: () => this.authService.getToken() ?? '',
          withCredentials: false,
        })
        .withAutomaticReconnect()
        .build();

      this.connection.on('OrderUpdated', (update: OrderUpdate) => this.handleUpdate(update));
    }

    if (!this.connectionStart) {
      this.connectionStart = this.connection.start().catch(err => {
        this.connectionStart = null;
        console.error('[OrderHubService] connection error:', err);
        throw err;
      });
    }

    try {
      await this.connectionStart;
    } catch {
      // Swallowed: updates just won't be live; the UI still works via manual refresh.
    }
  }

  private handleUpdate(update: OrderUpdate): void {
    this.lastUpdate.set(update);

    const userId = this.authService.user()?.id;
    if (userId) {
      // Refresh the shared signals so open-orders tables and wallet views update in place.
      this.tradeService.getOpenOrders(userId);
      this.walletService.getWalletOverview(userId);
    }

    const base = symbolToBaseAsset(update.pairSymbol);
    const verb = update.side === 'Buy' ? 'Bought' : 'Sold';
    const filled = update.status === 'Filled' ? 'filled' : 'partially filled';
    this.notif.success(
      `Order ${filled}: ${verb} ${update.lastFillQuantity} ${base} @ ${update.lastFillPrice}`
    );
  }
}
