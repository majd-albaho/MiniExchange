import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { NotificationService } from '../../../core/services/notification.service';
import { trigger, transition, style, animate } from '@angular/animations';

@Component({
  selector: 'app-notifications-toast',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  animations: [
    trigger('toastAnim', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateX(100%)' }),
        animate('200ms ease-out', style({ opacity: 1, transform: 'translateX(0)' })),
      ]),
      transition(':leave', [
        animate('150ms ease-in', style({ opacity: 0, transform: 'translateX(100%)' })),
      ]),
    ]),
  ],
  template: `
    <div class="toast-container">
      @for (n of notifService.notifications(); track n.id) {
        <div class="toast" [class]="'toast-' + n.type" @toastAnim>
          <mat-icon>{{ iconMap[n.type] }}</mat-icon>
          <span>{{ n.message }}</span>
          <button class="close-btn" (click)="notifService.dismiss(n.id)">
            <mat-icon>close</mat-icon>
          </button>
        </div>
      }
    </div>
  `,
  styles: [`
    .toast-container {
      position: fixed; bottom: 24px; right: 24px;
      display: flex; flex-direction: column; gap: 10px;
      z-index: 9999;
    }
    .toast {
      display: flex; align-items: center; gap: 10px;
      padding: 12px 16px; border-radius: 10px;
      min-width: 280px; max-width: 380px;
      font-size: 0.88rem; font-weight: 500;
      box-shadow: 0 4px 20px rgba(0,0,0,0.3);
    }
    .toast mat-icon { font-size: 20px; }
    .close-btn {
      margin-left: auto; background: transparent; border: none;
      cursor: pointer; color: inherit; opacity: 0.7; padding: 0;
    }
    .close-btn mat-icon { font-size: 16px; }
    .toast-success { background: #1a3a2a; color: #4caf50; border: 1px solid #4caf50; }
    .toast-error { background: #3a1a1a; color: #f44336; border: 1px solid #f44336; }
    .toast-info { background: #1a2a3a; color: #2196f3; border: 1px solid #2196f3; }
    .toast-warning { background: #3a2e1a; color: #ff9800; border: 1px solid #ff9800; }
  `],
})
export class NotificationsToastComponent {
  notifService = inject(NotificationService);
  iconMap: Record<string, string> = {
    success: 'check_circle',
    error: 'error',
    info: 'info',
    warning: 'warning',
  };
}
