import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AuthService } from '../../core/services/auth.service';
import { TransactionService } from '../../core/services/transaction.service';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';
import { Transaction, TransactionType } from '../../core/models/transaction.model';

@Component({
  selector: 'app-transactions',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatButtonModule, MatIconModule,
    MatTabsModule, MatFormFieldModule, MatSelectModule, MatInputModule,
    MatDatepickerModule, MatNativeDateModule, MatChipsModule, MatTooltipModule,
    PageHeaderComponent, LoadingSpinnerComponent,
  ],
  templateUrl: './transactions.component.html',
  styleUrl: './transactions.component.css',
})
export class TransactionsComponent implements OnInit {
  private authService = inject(AuthService);
  private txService = inject(TransactionService);

  loading = signal(true);
  transactions = signal<Transaction[]>([]);
  currentPage = signal(1);
  total = signal(0);
  totalPages = signal(1);
  expandedTx = signal<string | null>(null);

  activeTab = signal<TransactionType | 'all'>('all');
  statusFilter = 'all';
  symbolFilter = '';
  fromDate: Date | null = null;
  toDate: Date | null = null;

  readonly pageSize = 10;
  readonly symbols = ['BTC', 'ETH', 'SOL', 'USDT', 'BNB', 'ADA', 'XRP'];

  txIconMap: Record<string, string> = {
    send: 'arrow_upward',
    receive: 'arrow_downward',
    buy: 'shopping_cart',
    sell: 'sell',
  };

  private tabTypeMap: Record<number, TransactionType | 'all'> = {
    0: 'all',
    1: 'receive',
    2: 'send',
    3: 'buy',
    4: 'sell',
  };

  async ngOnInit(): Promise<void> {
    await this.loadTransactions();
  }

  onTabChange(index: number): void {
    this.activeTab.set(this.tabTypeMap[index]);
    this.currentPage.set(1);
    this.loadTransactions();
  }

  applyFilters(): void {
    this.currentPage.set(1);
    this.loadTransactions();
  }

  clearFilters(): void {
    this.statusFilter = 'all';
    this.symbolFilter = '';
    this.fromDate = null;
    this.toDate = null;
    this.applyFilters();
  }

  goToPage(page: number): void {
    this.currentPage.set(page);
    this.loadTransactions();
  }

  toggleDetails(id: string): void {
    this.expandedTx.update(v => v === id ? null : id);
  }

  copyText(text: string): void {
    navigator.clipboard.writeText(text);
  }

  private async loadTransactions(): Promise<void> {
    this.loading.set(true);
    const userId = this.authService.user()?.id ?? 'user-001';
    try {
      const res = await this.txService.getTransactions(userId, {
        type: this.activeTab(),
        status: this.statusFilter as any,
        symbol: this.symbolFilter || undefined,
        startDate: this.fromDate?.toISOString(),
        endDate: this.toDate?.toISOString(),
        page: this.currentPage(),
        pageSize: this.pageSize,
      });
      this.transactions.set(res.items);
      this.total.set(res.total);
      this.totalPages.set(Math.max(1, Math.ceil(res.total / this.pageSize)));
    } finally {
      this.loading.set(false);
    }
  }

  exportCSV(): void {
    const rows = this.transactions();
    if (!rows.length) return;
    const header = 'Type,Symbol,Amount,USDT Value,Fee,Status,Date,TxHash';
    const lines = rows.map(t =>
      `${t.type},${t.symbol},${t.amount},${t.amountUSDT},${t.fee} ${t.feeSymbol},${t.status},${t.createdAt},${t.txHash ?? ''}`
    );
    const csv = [header, ...lines].join('\n');
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url; a.download = 'transactions.csv'; a.click();
    URL.revokeObjectURL(url);
  }
}
