import { Injectable, signal } from '@angular/core';

export type NotificationType = 'success' | 'error' | 'info' | 'warning';

export interface AppNotification {
  id: string;
  message: string;
  type: NotificationType;
  duration?: number;
}

@Injectable({ providedIn: 'root' })
export class NotificationService {
  readonly notifications = signal<AppNotification[]>([]);

  show(message: string, type: NotificationType = 'info', duration = 4000): void {
    const notification: AppNotification = {
      id: Date.now().toString(),
      message,
      type,
      duration,
    };
    this.notifications.update(n => [...n, notification]);
    if (duration > 0) {
      setTimeout(() => this.dismiss(notification.id), duration);
    }
  }

  success(message: string): void {
    this.show(message, 'success');
  }

  error(message: string): void {
    this.show(message, 'error', 6000);
  }

  info(message: string): void {
    this.show(message, 'info');
  }

  warning(message: string): void {
    this.show(message, 'warning', 5000);
  }

  dismiss(id: string): void {
    this.notifications.update(n => n.filter(notif => notif.id !== id));
  }
}
