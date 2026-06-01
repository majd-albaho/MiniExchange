import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { AuthService } from '../../core/services/auth.service';
import { WalletService } from '../../core/services/wallet.service';
import { MarketService } from '../../core/services/market.service';
import { TransactionService } from '../../core/services/transaction.service';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { StatCardComponent } from '../../shared/components/stat-card/stat-card.component';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';
import { Transaction } from '../../core/models/transaction.model';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    CommonModule, RouterLink, MatButtonModule, MatIconModule,
    MatTableModule, PageHeaderComponent, StatCardComponent, LoadingSpinnerComponent,
  ],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css',
})
export class HomeComponent implements OnInit {
  authService = inject(AuthService);
  private walletService = inject(WalletService);
  private marketService = inject(MarketService);
  private transactionService = inject(TransactionService);

  loading = signal(true);
  wallet = this.walletService.walletOverview;
  tickers = this.marketService.tickers;
  recentTx = signal<Transaction[]>([]);

  txIconMap: Record<string, string> = {
    send: 'arrow_upward',
    receive: 'arrow_downward',
    buy: 'shopping_cart',
    sell: 'sell',
  };

  async ngOnInit(): Promise<void> {
    const userId = this.authService.user()?.id ?? 'user-001';
    try {
      await Promise.all([
        this.walletService.getWalletOverview(userId),
        this.marketService.getTickers(),
        this.loadRecentTx(userId),
      ]);
    } finally {
      this.loading.set(false);
    }
  }

  private async loadRecentTx(userId: string): Promise<void> {
    const res = await this.transactionService.getTransactions(userId, {
      type: 'all', page: 1, pageSize: 5,
    });
    this.recentTx.set(res.items);
  }
}
