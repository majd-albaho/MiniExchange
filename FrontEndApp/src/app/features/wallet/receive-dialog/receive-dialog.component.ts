import { Component, Inject, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { FormsModule } from '@angular/forms';
import { QRCodeComponent } from 'angularx-qrcode';
import { WalletService } from '../../../core/services/wallet.service';
import { NotificationService } from '../../../core/services/notification.service';
import { WalletAsset, ReceiveInfo } from '../../../core/models/wallet.model';

export interface ReceiveDialogData {
  asset: WalletAsset;
  allAssets: WalletAsset[];
  userId: string;
}

@Component({
  selector: 'app-receive-dialog',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    MatDialogModule, MatButtonModule, MatIconModule,
    MatSelectModule, MatFormFieldModule, QRCodeComponent,
  ],
  templateUrl: './receive-dialog.component.html',
  styleUrl: './receive-dialog.component.css',
})
export class ReceiveDialogComponent implements OnInit {
  private walletService = inject(WalletService);
  private notif = inject(NotificationService);

  selectedSymbol: string;
  selectedNetwork: string;
  receiveInfo = signal<ReceiveInfo | null>(null);
  loading = signal(false);
  copied = signal(false);
  networkOptions = signal<string[]>([]);

  private networkMap: Record<string, string[]> = {
    BTC: ['Bitcoin (BTC)'],
    ETH: ['Ethereum (ERC-20)', 'Arbitrum', 'Optimism'],
    USDT: ['Tron (TRC-20)', 'Ethereum (ERC-20)', 'BSC (BEP-20)'],
    SOL: ['Solana'],
  };

  constructor(
    public dialogRef: MatDialogRef<ReceiveDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ReceiveDialogData
  ) {
    this.selectedSymbol = data.asset.symbol;
    const nets = this.networkMap[this.selectedSymbol] ?? ['Mainnet'];
    this.networkOptions.set(nets);
    this.selectedNetwork = nets[0];
  }

  async ngOnInit(): Promise<void> {
    await this.loadReceiveInfo();
  }

  async onAssetChange(symbol: string): Promise<void> {
    const nets = this.networkMap[symbol] ?? ['Mainnet'];
    this.networkOptions.set(nets);
    this.selectedNetwork = nets[0];
    await this.loadReceiveInfo();
  }

  async loadReceiveInfo(): Promise<void> {
    this.loading.set(true);
    try {
      const info = await this.walletService.getReceiveInfo(
        this.data.userId, this.selectedSymbol, this.selectedNetwork
      );
      this.receiveInfo.set(info);
    } finally {
      this.loading.set(false);
    }
  }

  copyAddress(): void {
    const address = this.receiveInfo()?.address;
    if (address) {
      navigator.clipboard.writeText(address);
      this.copied.set(true);
      this.notif.success('Address copied to clipboard!');
      setTimeout(() => this.copied.set(false), 2000);
    }
  }

  copyMemo(): void {
    const memo = this.receiveInfo()?.memo;
    if (memo) {
      navigator.clipboard.writeText(memo);
      this.notif.success('Memo copied to clipboard!');
    }
  }

  close(): void {
    this.dialogRef.close();
  }
}
