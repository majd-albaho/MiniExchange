import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import {
  Transaction,
  TransactionFilter,
  PaginatedTransactions,
} from '../models/transaction.model';
import { firstValueFrom } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class TransactionService {
  private readonly baseUrl = environment.apiBase.transactions;

  readonly transactions = signal<PaginatedTransactions | null>(null);

  constructor(private http: HttpClient) {}

  async getTransactions(
    userId: string,
    filter: TransactionFilter
  ): Promise<PaginatedTransactions> {
    try {
      const res = await firstValueFrom(
        this.http.get<PaginatedTransactions>(`${this.baseUrl}/transactions/${userId}`, {
          params: filter as unknown as Record<string, string | number>,
        })
      );
      this.transactions.set(res);
      return res;
    } catch (err) {
      console.error('[TransactionService] getTransactions error:', err);
      const dummy: Transaction[] = [
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
        {
          id: 'tx-005', type: 'receive', status: 'pending', symbol: 'USDT',
          amount: 500, amountUSDT: 500, fee: 1, feeSymbol: 'USDT',
          fromAddress: 'TN3W4H6rK2ce4vX9YnFQHwKENnHjoxb3m9', network: 'TRC-20',
          createdAt: '2024-01-11T12:00:00Z', updatedAt: '2024-01-11T12:00:00Z',
        },
        {
          id: 'tx-006', type: 'buy', status: 'filled', symbol: 'ETH',
          amount: 1, amountUSDT: 2440.82, fee: 1.22, feeSymbol: 'USDT',
          pair: 'ETH/USDT', price: 2440.82, createdAt: '2024-01-10T08:30:00Z', updatedAt: '2024-01-10T08:30:02Z',
        },
      ];

      const filtered = dummy.filter(t => {
        if (filter.type && filter.type !== 'all' && t.type !== filter.type) return false;
        if (filter.status && filter.status !== 'all' && t.status !== filter.status) return false;
        if (filter.symbol && t.symbol !== filter.symbol) return false;
        return true;
      });

      const result: PaginatedTransactions = {
        items: filtered.slice((filter.page - 1) * filter.pageSize, filter.page * filter.pageSize),
        total: filtered.length,
        page: filter.page,
        pageSize: filter.pageSize,
      };
      this.transactions.set(result);
      return result;
    }
  }
}
