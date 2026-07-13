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
import { AuthService } from '../../../core/services/auth.service';
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
  templateUrl: './send-dialog.component.html',
  styleUrl: './send-dialog.component.css',
})
export class SendDialogComponent implements OnInit {
  private fb = inject(FormBuilder);
  private walletService = inject(WalletService);
  private notif = inject(NotificationService);
  private authService = inject(AuthService);

  loading = signal(false);
  selectedAsset = signal<WalletAsset>(null!);
  networkOptions = signal<string[]>([]);
  hasMemo = signal(false);
  isDemoSelected = signal(false);

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
    this.isDemoSelected.set(!!this.data.asset.isDemo);
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
      this.isDemoSelected.set(!!asset.isDemo);
      this.setNetworkOptions(symbol);
    }
  }

  setMax(): void {
    this.form.get('amount')?.setValue(this.selectedAsset().balance);
  }

  async onSubmit(): Promise<void> {
    if (this.isDemoSelected()) {
      this.notif.error(`${this.selectedAsset().symbol} is a demo token and cannot be sent on-chain.`);
      return;
    }
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading.set(true);
    try {
      const v = this.form.value;
      const userId = this.authService.user()?.id ?? 'user-001';
      const res = await this.walletService.sendCrypto(userId, {
        fromSymbol: v.symbol!,
        toAddress: v.toAddress!,
        amount: v.amount!,
        network: v.network!,
        pin: v.pin!,
        memo: v.memo || undefined,
      });
      this.notif.success(`Transaction submitted! TX: ${res.txId}`);
      this.dialogRef.close(true);
    } catch (err: any) {
      const message = err?.error?.message ?? 'Transaction failed. Please try again.';
      this.notif.error(message);
    } finally {
      this.loading.set(false);
    }
  }

  close(): void {
    this.dialogRef.close(false);
  }
}
