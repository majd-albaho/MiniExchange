import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { WalletService } from '../../core/services/wallet.service';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';
import { WalletAsset } from '../../core/models/wallet.model';
import { SendDialogComponent } from './send-dialog/send-dialog.component';
import { ReceiveDialogComponent } from './receive-dialog/receive-dialog.component';
import { AddDemoTokenDialogComponent } from './add-demo-token-dialog/add-demo-token-dialog.component';

@Component({
  selector: 'app-wallet',
  standalone: true,
  imports: [
    CommonModule, MatButtonModule, MatIconModule, MatTooltipModule,
    RouterLink, PageHeaderComponent, LoadingSpinnerComponent,
  ],
  templateUrl: './wallet.component.html',
  styleUrl: './wallet.component.css',
})
export class WalletComponent implements OnInit {
  private authService = inject(AuthService);
  walletService = inject(WalletService);
  private dialog = inject(MatDialog);

  loading = signal(true);
  searchQuery = '';
  filteredAssets = signal<WalletAsset[]>([]);

  async ngOnInit(): Promise<void> {
    await this.refresh();
    this.loading.set(false);
  }

  private async refresh(): Promise<void> {
    const userId = this.authService.user()?.id ?? 'user-001';
    await this.walletService.getWalletOverview(userId);
    this.filteredAssets.set(this.walletService.walletOverview()?.assets ?? []);
  }

  onSearch(event: Event): void {
    const query = (event.target as HTMLInputElement).value.toLowerCase();
    const all = this.walletService.walletOverview()?.assets ?? [];
    this.filteredAssets.set(
      all.filter(a => a.symbol.toLowerCase().includes(query) || a.name.toLowerCase().includes(query))
    );
  }

  openSendDialog(asset?: WalletAsset): void {
    const allAssets = this.walletService.walletOverview()?.assets ?? [];
    const target = asset ?? allAssets[0];
    if (!target) return;
    this.dialog.open(SendDialogComponent, {
      data: { asset: target, allAssets },
      panelClass: 'dark-dialog',
    }).afterClosed().subscribe(sent => {
      if (sent) this.refresh();
    });
  }

  openAddDemoTokenDialog(): void {
    const userId = this.authService.user()?.id ?? 'user-001';
    this.dialog.open(AddDemoTokenDialogComponent, {
      data: { userId },
      panelClass: 'dark-dialog',
    }).afterClosed().subscribe(added => {
      if (added) this.refresh();
    });
  }

  openReceiveDialog(asset?: WalletAsset): void {
    const allAssets = this.walletService.walletOverview()?.assets ?? [];
    const target = asset ?? allAssets[0];
    if (!target) return;
    const userId = this.authService.user()?.id ?? 'user-001';
    this.dialog.open(ReceiveDialogComponent, {
      data: { asset: target, allAssets, userId },
      panelClass: 'dark-dialog',
    });
  }
}
