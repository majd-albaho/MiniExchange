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
  template: `
    <div class="dialog-container">
      <div class="dialog-header">
        <h2>Receive {{ selectedSymbol }}</h2>
        <button mat-icon-button (click)="close()"><mat-icon>close</mat-icon></button>
      </div>

      <div class="dialog-body">
        <div class="asset-selector">
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Select Asset</mat-label>
            <mat-select [(ngModel)]="selectedSymbol" (ngModelChange)="onAssetChange($event)">
              @for (a of data.allAssets; track a.id) {
                <mat-option [value]="a.symbol">{{ a.name }} ({{ a.symbol }})</mat-option>
              }
            </mat-select>
          </mat-form-field>

          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Network</mat-label>
            <mat-select [(ngModel)]="selectedNetwork" (ngModelChange)="loadReceiveInfo()">
              @for (n of networkOptions(); track n) {
                <mat-option [value]="n">{{ n }}</mat-option>
              }
            </mat-select>
          </mat-form-field>
        </div>

        @if (loading()) {
          <div class="loading-placeholder">Loading address...</div>
        } @else if (receiveInfo()) {
          <div class="qr-section">
            <qrcode
              [qrdata]="receiveInfo()!.address"
              [width]="200"
              [errorCorrectionLevel]="'M'"
              [colorDark]="'#ffffff'"
              [colorLight]="'#1a1f2e'"
              [margin]="2"
            />
          </div>

          <div class="address-section">
            <div class="address-label">Deposit Address</div>
            <div class="address-box">
              <span class="address-text">{{ receiveInfo()!.address }}</span>
              <button mat-icon-button (click)="copyAddress()">
                <mat-icon>{{ copied() ? 'check' : 'content_copy' }}</mat-icon>
              </button>
            </div>
          </div>

          @if (receiveInfo()!.memo) {
            <div class="address-section">
              <div class="address-label">Memo / Tag (Required)</div>
              <div class="address-box">
                <span class="address-text">{{ receiveInfo()!.memo }}</span>
                <button mat-icon-button (click)="copyMemo()">
                  <mat-icon>content_copy</mat-icon>
                </button>
              </div>
            </div>
          }

          <div class="tips-section">
            <h4>⚠️ Important Notes</h4>
            <ul>
              <li>Only send <strong>{{ selectedSymbol }}</strong> to this address on the <strong>{{ selectedNetwork }}</strong> network.</li>
              <li>Minimum deposit: <strong>{{ receiveInfo()!.minDeposit }}</strong> {{ selectedSymbol }}</li>
              <li>Required confirmations: <strong>{{ receiveInfo()!.confirmations }}</strong></li>
              <li>Sending unsupported assets or wrong network may result in permanent loss.</li>
            </ul>
          </div>
        }
      </div>

      <div class="dialog-actions">
        <button mat-raised-button color="primary" (click)="copyAddress()" [disabled]="!receiveInfo()">
          <mat-icon>content_copy</mat-icon> Copy Address
        </button>
        <button mat-button (click)="close()">Close</button>
      </div>
    </div>
  `,
  styles: [`
    .dialog-container { width: 460px; padding: 24px; background: var(--bg-card); color: var(--text-primary); }
    .dialog-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 20px; }
    .dialog-header h2 { margin: 0; font-size: 1.2rem; font-weight: 700; }
    .dialog-body { display: flex; flex-direction: column; gap: 16px; }
    .asset-selector { display: flex; flex-direction: column; gap: 4px; }
    .full-width { width: 100%; }
    .qr-section { display: flex; justify-content: center; padding: 16px; background: #1a1f2e; border-radius: 12px; border: 1px solid var(--border-color); }
    .address-section { display: flex; flex-direction: column; gap: 6px; }
    .address-label { font-size: 0.78rem; color: var(--text-secondary); text-transform: uppercase; letter-spacing: 0.5px; }
    .address-box {
      display: flex; align-items: center; gap: 8px;
      background: var(--bg-primary); border: 1px solid var(--border-color);
      border-radius: 8px; padding: 10px 12px;
    }
    .address-text { flex: 1; font-family: monospace; font-size: 0.82rem; color: var(--text-primary); word-break: break-all; }
    .tips-section { background: rgba(255,152,0,0.08); border: 1px solid rgba(255,152,0,0.3); border-radius: 10px; padding: 14px 16px; }
    .tips-section h4 { margin: 0 0 10px; font-size: 0.88rem; color: #ff9800; }
    .tips-section ul { margin: 0; padding-left: 16px; display: flex; flex-direction: column; gap: 6px; }
    .tips-section li { font-size: 0.82rem; color: var(--text-secondary); }
    .tips-section strong { color: var(--text-primary); }
    .dialog-actions { display: flex; justify-content: space-between; align-items: center; margin-top: 16px; }
    .loading-placeholder { text-align: center; padding: 40px; color: var(--text-secondary); }
  `],
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
