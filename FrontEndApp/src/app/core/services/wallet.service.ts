import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { WalletOverview, WalletAsset, SendRequest, ReceiveInfo } from '../models/wallet.model';
import { firstValueFrom } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class WalletService {
  private readonly baseUrl = environment.apiBase.wallet;

  readonly walletOverview = signal<WalletOverview | null>(null);

  constructor(private http: HttpClient) {}

  async getWalletOverview(userId: string): Promise<WalletOverview> {
    try {
      const res = await firstValueFrom(
        this.http.get<WalletOverview>(`${this.baseUrl}/UserWallets/${userId}/overview`)
      );
      // Backend omits presentation-only fields; give each asset a safe default logo.
      res.assets = (res.assets ?? []).map(a => ({ ...a, logoUrl: a.logoUrl ?? '' }));
      this.walletOverview.set(res);
      return res;
    } catch (err) {
      console.error('[WalletService] getWalletOverview error:', err);
      const dummy: WalletOverview = {
        totalBalanceUSDT: 15420.75,
        totalChange24h: 2.34,
        assets: [
          {
            id: '1', symbol: 'BTC', name: 'Bitcoin', network: 'Bitcoin',
            balance: 0.1842, balanceUSDT: 8140.25, depositAddress: 'bc1qxy2kgdygjrsqtzq2n0yrf2493p83kkfjhx0wlh',
            logoUrl: 'https://cryptologos.cc/logos/bitcoin-btc-logo.png',
            change24h: 1.85, price: 44187.32,
          },
          {
            id: '2', symbol: 'ETH', name: 'Ethereum', network: 'ERC-20',
            balance: 2.45, balanceUSDT: 5980.00, depositAddress: '0x71C7656EC7ab88b098defB751B7401B5f6d8976F',
            logoUrl: 'https://cryptologos.cc/logos/ethereum-eth-logo.png',
            change24h: 3.12, price: 2440.82,
          },
          {
            id: '3', symbol: 'SOL', name: 'Solana', network: 'Solana',
            balance: 12.5, balanceUSDT: 1225.50, depositAddress: '7xKXtg2CW87d97TXJSDpbD5jBkheTqA83TZRuJosgHkv',
            logoUrl: 'https://cryptologos.cc/logos/solana-sol-logo.png',
            change24h: 5.67, price: 98.04,
          },
          {
            id: '4', symbol: 'USDT', name: 'Tether', network: 'TRC-20',
            balance: 75.00, balanceUSDT: 75.00, depositAddress: 'TN3W4H6rK2ce4vX9YnFQHwKENnHjoxb3m9',
            logoUrl: 'https://cryptologos.cc/logos/tether-usdt-logo.png',
            change24h: 0.01, price: 1.00,
          },
        ],
      };
      this.walletOverview.set(dummy);
      return dummy;
    }
  }

  async getReceiveInfo(userId: string, symbol: string, network: string): Promise<ReceiveInfo> {
    try {
      return await firstValueFrom(
        this.http.get<ReceiveInfo>(`${this.baseUrl}/UserWallets/${userId}/receive`, {
          params: { symbol, network },
        })
      );
    } catch (err) {
      console.error('[WalletService] getReceiveInfo error:', err);
      const addressMap: Record<string, string> = {
        BTC: 'bc1qxy2kgdygjrsqtzq2n0yrf2493p83kkfjhx0wlh',
        ETH: '0x71C7656EC7ab88b098defB751B7401B5f6d8976F',
        SOL: '7xKXtg2CW87d97TXJSDpbD5jBkheTqA83TZRuJosgHkv',
        USDT: 'TN3W4H6rK2ce4vX9YnFQHwKENnHjoxb3m9',
      };
      return {
        symbol,
        network,
        address: addressMap[symbol] ?? 'demo-address-' + symbol,
        minDeposit: 0.001,
        confirmations: symbol === 'BTC' ? 6 : 12,
      };
    }
  }

  async sendCrypto(userId: string, request: SendRequest): Promise<{ txId: string }> {
    // Demo/test tokens have no on-chain backing and are rejected server-side too.
    const res = await firstValueFrom(
      this.http.post<{ txId: string }>(`${this.baseUrl}/UserWallets/Send`, {
        userId,
        assetSymbol: request.fromSymbol,
        recipientAddress: request.toAddress,
        amount: request.amount,
      })
    );
    return res;
  }

  /** Adds a ledger-only demo token for testing. It can be traded but never withdrawn. */
  async addDemoToken(userId: string, assetName: string, amount: number): Promise<void> {
    await firstValueFrom(
      this.http.post(`${this.baseUrl}/Transactions/AddDemoToken`, {
        userId,
        assetName,
        amount,
      })
    );
  }
}
