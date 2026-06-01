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
  templateUrl: './send-dialog.component.html',
  styleUrl: './send-dialog.component.css',
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
