import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-stat-card',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="stat-card">
      <div class="stat-label">{{ label }}</div>
      <div class="stat-value">{{ value }}</div>
      @if (change !== undefined) {
        <div class="stat-change" [class.up]="change >= 0" [class.down]="change < 0">
          {{ change >= 0 ? '+' : '' }}{{ change | number:'1.2-2' }}%
        </div>
      }
    </div>
  `,
  styles: [`
    .stat-card {
      background: var(--bg-card);
      border: 1px solid var(--border-color);
      border-radius: 12px;
      padding: 20px;
    }
    .stat-label { font-size: 0.8rem; color: var(--text-secondary); margin-bottom: 8px; text-transform: uppercase; letter-spacing: 0.5px; }
    .stat-value { font-size: 1.5rem; font-weight: 700; color: var(--text-primary); }
    .stat-change { font-size: 0.85rem; font-weight: 600; margin-top: 4px; }
    .up { color: var(--success); }
    .down { color: var(--danger); }
  `],
})
export class StatCardComponent {
  @Input() label = '';
  @Input() value = '';
  @Input() change?: number;
}
