import { Component, Inject, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { WalletService } from '../../../core/services/wallet.service';
import { NotificationService } from '../../../core/services/notification.service';

export interface AddDemoTokenDialogData {
  userId: string;
}

@Component({
  selector: 'app-add-demo-token-dialog',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    MatDialogModule, MatFormFieldModule, MatInputModule,
    MatButtonModule, MatIconModule,
  ],
  templateUrl: './add-demo-token-dialog.component.html',
  styleUrl: './add-demo-token-dialog.component.css',
})
export class AddDemoTokenDialogComponent {
  private fb = inject(FormBuilder);
  private walletService = inject(WalletService);
  private notif = inject(NotificationService);

  loading = signal(false);

  form = this.fb.group({
    assetName: ['', [Validators.required, Validators.pattern(/^[A-Za-z0-9]{2,12}$/)]],
    amount: [null as number | null, [Validators.required, Validators.min(0.00000001)]],
  });

  constructor(
    public dialogRef: MatDialogRef<AddDemoTokenDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: AddDemoTokenDialogData
  ) {}

  async onSubmit(): Promise<void> {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading.set(true);
    try {
      const v = this.form.value;
      await this.walletService.addDemoToken(this.data.userId, v.assetName!.toUpperCase(), v.amount!);
      this.notif.success(`Added ${v.amount} demo ${v.assetName!.toUpperCase()} to your wallet.`);
      this.dialogRef.close(true);
    } catch (err: any) {
      const message = err?.error?.message ?? 'Failed to add demo token. Please try again.';
      this.notif.error(message);
    } finally {
      this.loading.set(false);
    }
  }

  close(): void {
    this.dialogRef.close(false);
  }
}
