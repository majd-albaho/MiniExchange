import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import {
  Transaction,
  TransactionFilter,
  PaginatedTransactions,
} from '../models/transaction.model';
import { firstValueFrom } from 'rxjs';
import { symbolToBaseAsset, symbolToPair } from '../models/market-symbol';

/** Wallet-ledger row (deposits/withdrawals + raw trade legs) from WalletService. */
interface WalletTxDto {
  id: string;
  type: Transaction['type'];
  status: Transaction['status'];
  symbol: string;
  amount: number;
  amountUSDT: number;
  fee: number;
  feeSymbol: string;
  txHash?: string;
  network?: string;
  createdAt: string;
  updatedAt: string;
}

/** One trade, already collapsed to a single row, from TradingService. */
interface TradeHistoryDto {
  tradeId: string;
  pairSymbol: string;
  side: 'Buy' | 'Sell';
  price: number;
  quantity: number;
  quoteAmount: number;
  executedAt: string;
}

// Fetch a generous window from each source and merge/paginate on the client. Fine for this
// sandbox's data volumes; a unified server-side history endpoint would replace this at scale.
const SOURCE_FETCH_SIZE = 200;

@Injectable({ providedIn: 'root' })
export class TransactionService {
  private readonly walletUrl = environment.apiBase.transactions;
  private readonly tradeUrl = environment.apiBase.trade;

  readonly transactions = signal<PaginatedTransactions | null>(null);

  constructor(private http: HttpClient) {}

  async getTransactions(
    userId: string,
    filter: TransactionFilter
  ): Promise<PaginatedTransactions> {
    try {
      const [walletRes, tradeRes] = await Promise.all([
        firstValueFrom(
          this.http.get<PaginatedTransactions>(`${this.walletUrl}/Transactions/user/${userId}`, {
            params: { page: 1, pageSize: SOURCE_FETCH_SIZE },
          })
        ),
        firstValueFrom(
          this.http.get<{ items: TradeHistoryDto[] }>(`${this.tradeUrl}/trades/user/${userId}`, {
            params: { page: 1, pageSize: SOURCE_FETCH_SIZE },
          })
        ),
      ]);

      // Transfers come from the wallet ledger; trades come from TradingService as single rows,
      // so drop the wallet's two-legged trade entries (buy/sell) to avoid double-counting.
      const transfers = (walletRes.items as WalletTxDto[])
        .filter(t => t.type === 'receive' || t.type === 'send');

      const trades = (tradeRes.items ?? []).map(t => this.mapTrade(t));

      const merged = [...transfers, ...trades].sort(
        (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
      );

      const result = this.applyFilterAndPaginate(merged, filter);
      this.transactions.set(result);
      return result;
    } catch (err) {
      console.error('[TransactionService] getTransactions error:', err);
      const result = this.applyFilterAndPaginate(this.dummyData(), filter);
      this.transactions.set(result);
      return result;
    }
  }

  private mapTrade(t: TradeHistoryDto): Transaction {
    const side = t.side.toLowerCase() as 'buy' | 'sell';
    return {
      id: 'trade-' + t.tradeId,
      type: side,
      status: 'completed',
      symbol: symbolToBaseAsset(t.pairSymbol),
      amount: t.quantity,
      amountUSDT: t.quoteAmount,
      fee: 0,
      feeSymbol: 'USDT',
      pair: symbolToPair(t.pairSymbol),
      price: t.price,
      createdAt: t.executedAt,
      updatedAt: t.executedAt,
    };
  }

  private applyFilterAndPaginate(
    rows: Transaction[],
    filter: TransactionFilter
  ): PaginatedTransactions {
    const start = filter.startDate ? new Date(filter.startDate).getTime() : null;
    const end = filter.endDate ? new Date(filter.endDate).getTime() : null;

    const filtered = rows.filter(t => {
      if (filter.type && filter.type !== 'all' && t.type !== filter.type) return false;
      if (filter.status && filter.status !== 'all' && t.status !== filter.status) return false;
      if (filter.symbol && t.symbol !== filter.symbol) return false;
      const time = new Date(t.createdAt).getTime();
      if (start !== null && time < start) return false;
      if (end !== null && time > end) return false;
      return true;
    });

    return {
      items: filtered.slice((filter.page - 1) * filter.pageSize, filter.page * filter.pageSize),
      total: filtered.length,
      page: filter.page,
      pageSize: filter.pageSize,
    };
  }

  private dummyData(): Transaction[] {
    return [
      {
        id: 'tx-001', type: 'receive', status: 'completed', symbol: 'BTC',
        amount: 0.05, amountUSDT: 2209.37, fee: 0.0001, feeSymbol: 'BTC',
        fromAddress: '1A1zP1eP5QGefi2DMPTfTL5SLmv7Divf', txHash: 'abc123...def456',
        network: 'Bitcoin', createdAt: '2024-01-15T10:30:00Z', updatedAt: '2024-01-15T11:00:00Z',
      },
      {
        id: 'tx-002', type: 'send', status: 'completed', symbol: 'ETH',
        amount: 0.5, amountUSDT: 1220.41, fee: 0.002, feeSymbol: 'ETH',
        toAddress: '0xAbCdEf...123456', txHash: 'xyz789...uvw012',
        network: 'ERC-20', createdAt: '2024-01-14T14:20:00Z', updatedAt: '2024-01-14T14:25:00Z',
      },
      {
        id: 'tx-003', type: 'buy', status: 'completed', symbol: 'BTC',
        amount: 0.025, amountUSDT: 1104.68, fee: 0.5, feeSymbol: 'USDT',
        pair: 'BTC/USDT', price: 44187.32, createdAt: '2024-01-13T09:15:00Z', updatedAt: '2024-01-13T09:15:05Z',
      },
      {
        id: 'tx-004', type: 'sell', status: 'completed', symbol: 'SOL',
        amount: 5, amountUSDT: 490.2, fee: 0.25, feeSymbol: 'USDT',
        pair: 'SOL/USDT', price: 98.04, createdAt: '2024-01-12T16:45:00Z', updatedAt: '2024-01-12T16:45:03Z',
      },
    ];
  }
}
