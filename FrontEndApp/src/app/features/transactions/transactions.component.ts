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
  template: `
    <div class="transactions-page">
      <app-page-header title="Transaction History" subtitle="All your deposits, withdrawals and trades">
        <button mat-stroked-button (click)="exportCSV()">
          <mat-icon>download</mat-icon> Export CSV
        </button>
      </app-page-header>

      <!-- Tabs -->
      <mat-tab-group (selectedTabChange)="onTabChange($event.index)" animationDuration="200ms">
        <mat-tab label="All"></mat-tab>
        <mat-tab label="Deposits"></mat-tab>
        <mat-tab label="Withdrawals"></mat-tab>
        <mat-tab label="Buy Orders"></mat-tab>
        <mat-tab label="Sell Orders"></mat-tab>
      </mat-tab-group>

      <!-- Filters -->
      <div class="filters-bar">
        <mat-form-field appearance="outline" class="filter-field">
          <mat-label>Status</mat-label>
          <mat-select [(value)]="statusFilter" (selectionChange)="applyFilters()">
            <mat-option value="all">All</mat-option>
            <mat-option value="completed">Completed</mat-option>
            <mat-option value="pending">Pending</mat-option>
            <mat-option value="failed">Failed</mat-option>
            <mat-option value="cancelled">Cancelled</mat-option>
          </mat-select>
        </mat-form-field>

        <mat-form-field appearance="outline" class="filter-field">
          <mat-label>Asset</mat-label>
          <mat-select [(value)]="symbolFilter" (selectionChange)="applyFilters()">
            <mat-option value="">All Assets</mat-option>
            @for (sym of symbols; track sym) {
              <mat-option [value]="sym">{{ sym }}</mat-option>
            }
          </mat-select>
        </mat-form-field>

        <mat-form-field appearance="outline" class="filter-field">
          <mat-label>From Date</mat-label>
          <input matInput [matDatepicker]="fromPicker" [value]="fromDate" (dateChange)="fromDate = $event.value; applyFilters()" />
          <mat-datepicker-toggle matIconSuffix [for]="fromPicker" />
          <mat-datepicker #fromPicker />
        </mat-form-field>

        <mat-form-field appearance="outline" class="filter-field">
          <mat-label>To Date</mat-label>
          <input matInput [matDatepicker]="toPicker" [value]="toDate" (dateChange)="toDate = $event.value; applyFilters()" />
          <mat-datepicker-toggle matIconSuffix [for]="toPicker" />
          <mat-datepicker #toPicker />
        </mat-form-field>

        <button mat-stroked-button (click)="clearFilters()" class="clear-btn">
          <mat-icon>clear</mat-icon> Clear
        </button>
      </div>

      <!-- Results -->
      @if (loading()) {
        <app-loading-spinner message="Loading transactions..." />
      } @else {
        <div class="tx-table">
          <div class="table-header">
            <span>Type</span>
            <span>Asset</span>
            <span>Amount</span>
            <span>Value (USDT)</span>
            <span>Fee</span>
            <span>Status</span>
            <span>Date</span>
            <span>Details</span>
          </div>

          @if (transactions().length === 0) {
            <div class="empty-state">
              <mat-icon>receipt_long</mat-icon>
              <p>No transactions found for the selected filters.</p>
            </div>
          }

          @for (tx of transactions(); track tx.id) {
            <div class="table-row" (click)="toggleDetails(tx.id)">
              <div class="type-cell">
                <div class="type-badge" [class]="'badge-' + tx.type">
                  <mat-icon>{{ txIconMap[tx.type] }}</mat-icon>
                  {{ tx.type | titlecase }}
                </div>
              </div>
              <div class="asset-cell">
                <div class="asset-symbol">{{ tx.symbol }}</div>
                @if (tx.network) {
                  <div class="asset-network">{{ tx.network }}</div>
                }
                @if (tx.pair) {
                  <div class="asset-network">{{ tx.pair }}</div>
                }
              </div>
              <div class="amount-cell" [class]="tx.type === 'receive' || tx.type === 'buy' ? 'up' : 'down'">
                {{ tx.type === 'receive' || tx.type === 'buy' ? '+' : '-' }}{{ tx.amount | number:'1.4-8' }} {{ tx.symbol }}
              </div>
              <div class="usdt-cell">\${{ tx.amountUSDT | number:'1.2-2' }}</div>
              <div class="fee-cell">{{ tx.fee }} {{ tx.feeSymbol }}</div>
              <div class="status-cell">
                <span class="status-badge" [class]="'status-' + tx.status">{{ tx.status }}</span>
              </div>
              <div class="date-cell">
                <div>{{ tx.createdAt | date:'MMM d, y' }}</div>
                <div class="time">{{ tx.createdAt | date:'HH:mm' }}</div>
              </div>
              <div class="detail-cell">
                <button mat-icon-button (click)="$event.stopPropagation(); toggleDetails(tx.id)">
                  <mat-icon>{{ expandedTx() === tx.id ? 'expand_less' : 'expand_more' }}</mat-icon>
                </button>
              </div>

              <!-- Expanded Detail -->
              @if (expandedTx() === tx.id) {
                <div class="tx-detail" (click)="$event.stopPropagation()">
                  @if (tx.txHash) {
                    <div class="detail-row">
                      <span>TX Hash:</span>
                      <code>{{ tx.txHash }}</code>
                      <button mat-icon-button matTooltip="Copy" (click)="copyText(tx.txHash!)">
                        <mat-icon>content_copy</mat-icon>
                      </button>
                    </div>
                  }
                  @if (tx.fromAddress) {
                    <div class="detail-row">
                      <span>From:</span>
                      <code>{{ tx.fromAddress }}</code>
                    </div>
                  }
                  @if (tx.toAddress) {
                    <div class="detail-row">
                      <span>To:</span>
                      <code>{{ tx.toAddress }}</code>
                    </div>
                  }
                  @if (tx.price) {
                    <div class="detail-row">
                      <span>Avg Price:</span>
                      <span>\${{ tx.price | number:'1.2-6' }}</span>
                    </div>
                  }
                </div>
              }
            </div>
          }
        </div>

        <!-- Pagination -->
        <div class="pagination">
          <button mat-icon-button [disabled]="currentPage() === 1" (click)="goToPage(currentPage() - 1)">
            <mat-icon>chevron_left</mat-icon>
          </button>
          <span class="page-info">Page {{ currentPage() }} of {{ totalPages() }}</span>
          <button mat-icon-button [disabled]="currentPage() >= totalPages()" (click)="goToPage(currentPage() + 1)">
            <mat-icon>chevron_right</mat-icon>
          </button>
          <span class="total-info">{{ total() }} total</span>
        </div>
      }
    </div>
  `,
  styles: [`
    .transactions-page { display: flex; flex-direction: column; gap: 16px; }
    mat-tab-group { background: var(--bg-card); border-radius: 12px; border: 1px solid var(--border-color); }
    .filters-bar {
      display: flex; flex-wrap: wrap; gap: 12px; align-items: center;
      background: var(--bg-card); border: 1px solid var(--border-color);
      border-radius: 12px; padding: 16px;
    }
    .filter-field { min-width: 160px; }
    .clear-btn { height: 56px; }
    .tx-table { background: var(--bg-card); border: 1px solid var(--border-color); border-radius: 12px; overflow: hidden; }
    .table-header {
      display: grid;
      grid-template-columns: 140px 120px 160px 120px 100px 100px 120px 60px;
      padding: 10px 20px;
      font-size: 0.74rem; font-weight: 700; color: var(--text-secondary);
      text-transform: uppercase; letter-spacing: 0.5px;
      border-bottom: 1px solid var(--border-color);
      background: var(--bg-primary);
    }
    .table-row {
      display: grid;
      grid-template-columns: 140px 120px 160px 120px 100px 100px 120px 60px;
      padding: 14px 20px; align-items: center;
      border-bottom: 1px solid var(--border-color);
      cursor: pointer; transition: background 0.15s;
      position: relative;
    }
    .table-row:hover { background: var(--bg-primary); }
    .table-row:last-child { border-bottom: none; }
    .type-badge {
      display: inline-flex; align-items: center; gap: 4px;
      padding: 4px 10px; border-radius: 20px;
      font-size: 0.78rem; font-weight: 600;
    }
    .type-badge mat-icon { font-size: 14px; width: 14px; height: 14px; }
    .badge-receive, .badge-buy { background: rgba(76,175,80,0.15); color: var(--success); }
    .badge-send, .badge-sell { background: rgba(244,67,54,0.15); color: var(--danger); }
    .asset-symbol { font-size: 0.9rem; font-weight: 700; color: var(--text-primary); }
    .asset-network { font-size: 0.74rem; color: var(--text-secondary); }
    .amount-cell { font-size: 0.88rem; font-weight: 600; }
    .up { color: var(--success); }
    .down { color: var(--danger); }
    .usdt-cell { font-size: 0.88rem; color: var(--text-primary); }
    .fee-cell { font-size: 0.8rem; color: var(--text-secondary); }
    .status-badge { font-size: 0.75rem; padding: 3px 8px; border-radius: 12px; font-weight: 600; }
    .status-completed, .status-filled { background: rgba(76,175,80,0.15); color: var(--success); }
    .status-pending { background: rgba(255,152,0,0.15); color: #ff9800; }
    .status-failed, .status-cancelled { background: rgba(244,67,54,0.15); color: var(--danger); }
    .date-cell { font-size: 0.83rem; color: var(--text-primary); }
    .time { font-size: 0.75rem; color: var(--text-secondary); }
    .tx-detail {
      grid-column: 1 / -1;
      background: var(--bg-primary); border-radius: 8px;
      padding: 14px 16px; margin-top: 8px;
      display: flex; flex-direction: column; gap: 8px;
    }
    .detail-row {
      display: flex; align-items: center; gap: 10px; font-size: 0.82rem;
      color: var(--text-secondary);
    }
    .detail-row code { font-family: monospace; color: var(--accent); flex: 1; word-break: break-all; }
    .detail-row button { width: 28px; height: 28px; }
    .empty-state { display: flex; flex-direction: column; align-items: center; padding: 60px; gap: 12px; color: var(--text-secondary); }
    .empty-state mat-icon { font-size: 48px; width: 48px; height: 48px; opacity: 0.3; }
    .pagination {
      display: flex; align-items: center; justify-content: center; gap: 10px;
      padding: 16px; color: var(--text-secondary); font-size: 0.85rem;
    }
    .page-info { font-weight: 600; color: var(--text-primary); }
    .total-info { margin-left: 16px; color: var(--text-secondary); }
  `],
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
