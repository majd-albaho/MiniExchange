import { Component, Inject, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { WalletService } from '../../../core/services/wallet.service';
import { NotificationService } from '../../../core/services/notification.service';
import { WalletAsset } from '../../../core/models/wallet.model';

export interface SendDialogData {
  asset: WalletAsset;
  allAssets: WalletAsset[];
}

@Component({
  selector: 'app-send-dialog',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    MatDialogModule, MatFormFieldModule, MatInputModule,
    MatButtonModule, MatIconModule, MatSelectModule,
  ],
  template: `
    <div class="dialog-container">
      <div class="dialog-header">
        <h2>Send {{ selectedAsset().symbol }}</h2>
        <button mat-icon-button (click)="close()"><mat-icon>close</mat-icon></button>
      </div>

      <form [formGroup]="form" (ngSubmit)="onSubmit()" class="dialog-form">

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Asset</mat-label>
          <mat-select formControlName="symbol" (selectionChange)="onAssetChange($event.value)">
            @for (a of data.allAssets; track a.id) {
              <mat-option [value]="a.symbol">
                {{ a.symbol }} — {{ a.balance | number:'1.4-8' }} available
              </mat-option>
            }
          </mat-select>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Network</mat-label>
          <mat-select formControlName="network">
            @for (n of networkOptions(); track n) {
              <mat-option [value]="n">{{ n }}</mat-option>
            }
          </mat-select>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Recipient Address</mat-label>
          <mat-icon matPrefix>send</mat-icon>
          <input matInput formControlName="toAddress" placeholder="Paste recipient address" />
          @if (form.get('toAddress')?.hasError('required') && form.get('toAddress')?.touched) {
            <mat-error>Address is required</mat-error>
          }
        </mat-form-field>

        @if (hasMemo()) {
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Memo / Tag (optional)</mat-label>
            <input matInput formControlName="memo" placeholder="Required for some exchanges" />
          </mat-form-field>
        }

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Amount</mat-label>
          <input matInput formControlName="amount" type="number" step="any" />
          <button mat-button matSuffix type="button" (click)="setMax()">MAX</button>
          @if (form.get('amount')?.hasError('required') && form.get('amount')?.touched) {
            <mat-error>Amount is required</mat-error>
          }
          @if (form.get('amount')?.hasError('min') && form.get('amount')?.touched) {
            <mat-error>Amount must be greater than 0</mat-error>
          }
          <mat-hint>≈ \${{ estimatedUSDT() | number:'1.2-2' }} USDT</mat-hint>
        </mat-form-field>

        <div class="fee-info">
          <span>Network Fee:</span>
          <span>~0.0001 {{ selectedAsset().symbol }}</span>
        </div>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>PIN</mat-label>
          <mat-icon matPrefix>lock</mat-icon>
          <input matInput formControlName="pin" type="password" maxlength="6" placeholder="Enter your transaction PIN" />
          @if (form.get('pin')?.hasError('required') && form.get('pin')?.touched) {
            <mat-error>PIN is required</mat-error>
          }
        </mat-form-field>

        <div class="dialog-actions">
          <button mat-button type="button" (click)="close()">Cancel</button>
          <button mat-raised-button color="warn" type="submit" [disabled]="loading()">
            @if (loading()) { <span class="btn-spinner"></span> } Send
          </button>
        </div>
      </form>
    </div>
  `,
  styles: [`
    .dialog-container { width: 460px; padding: 24px; background: var(--bg-card); color: var(--text-primary); }
    .dialog-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 20px; }
    .dialog-header h2 { margin: 0; font-size: 1.2rem; font-weight: 700; }
    .dialog-form { display: flex; flex-direction: column; gap: 8px; }
    .full-width { width: 100%; }
    .fee-info { display: flex; justify-content: space-between; font-size: 0.82rem; color: var(--text-secondary); padding: 4px 0; }
    .dialog-actions { display: flex; justify-content: flex-end; gap: 10px; margin-top: 8px; }
    .btn-spinner { display: inline-block; width: 14px; height: 14px; border: 2px solid rgba(255,255,255,0.3); border-top-color: white; border-radius: 50%; animation: spin 0.8s linear infinite; margin-right: 6px; }
    @keyframes spin { to { transform: rotate(360deg); } }
  `],
})
export class SendDialogComponent implements OnInit {
  private fb = inject(FormBuilder);
  private walletService = inject(WalletService);
  private notif = inject(NotificationService);

  loading = signal(false);
  selectedAsset = signal<WalletAsset>(null!);
  networkOptions = signal<string[]>([]);
  hasMemo = signal(false);

  form = this.fb.group({
    symbol: ['', Validators.required],
    network: ['', Validators.required],
    toAddress: ['', Validators.required],
    memo: [''],
    amount: [null as number | null, [Validators.required, Validators.min(0.000001)]],
    pin: ['', Validators.required],
  });

  estimatedUSDT = signal(0);

  constructor(
    public dialogRef: MatDialogRef<SendDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: SendDialogData
  ) {}

  ngOnInit(): void {
    this.selectedAsset.set(this.data.asset);
    this.form.get('symbol')?.setValue(this.data.asset.symbol);
    this.setNetworkOptions(this.data.asset.symbol);
    this.form.get('amount')?.valueChanges.subscribe(v => {
      const price = this.selectedAsset().price;
      this.estimatedUSDT.set((v ?? 0) * price);
    });
  }

  private setNetworkOptions(symbol: string): void {
    const networks: Record<string, string[]> = {
      BTC: ['Bitcoin (BTC)'],
      ETH: ['Ethereum (ERC-20)', 'Arbitrum', 'Optimism'],
      USDT: ['Tron (TRC-20)', 'Ethereum (ERC-20)', 'BSC (BEP-20)'],
      SOL: ['Solana'],
    };
    const opts = networks[symbol] ?? ['Mainnet'];
    this.networkOptions.set(opts);
    this.form.get('network')?.setValue(opts[0]);
    this.hasMemo.set(['XRP', 'XLM', 'EOS'].includes(symbol));
  }

  onAssetChange(symbol: string): void {
    const asset = this.data.allAssets.find(a => a.symbol === symbol);
    if (asset) {
      this.selectedAsset.set(asset);
      this.setNetworkOptions(symbol);
    }
  }

  setMax(): void {
    this.form.get('amount')?.setValue(this.selectedAsset().balance);
  }

  async onSubmit(): Promise<void> {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading.set(true);
    try {
      const v = this.form.value;
      const res = await this.walletService.sendCrypto({
        fromSymbol: v.symbol!,
        toAddress: v.toAddress!,
        amount: v.amount!,
        network: v.network!,
        pin: v.pin!,
        memo: v.memo || undefined,
      });
      this.notif.success(`Transaction submitted! TX: ${res.txId}`);
      this.dialogRef.close(true);
    } catch {
      this.notif.error('Transaction failed. Please try again.');
    } finally {
      this.loading.set(false);
    }
  }

  close(): void {
    this.dialogRef.close(false);
  }
}
