import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-loading-spinner',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="spinner-wrapper" [class.overlay]="overlay">
      <div class="spinner"></div>
      @if (message) {
        <p class="message">{{ message }}</p>
      }
    </div>
  `,
  styles: [`
    .spinner-wrapper {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 40px;
      gap: 12px;
    }
    .spinner-wrapper.overlay {
      position: fixed; inset: 0; background: rgba(0,0,0,0.5); z-index: 9998;
    }
    .spinner {
      width: 40px; height: 40px;
      border: 3px solid var(--border-color);
      border-top-color: var(--accent);
      border-radius: 50%;
      animation: spin 0.8s linear infinite;
    }
    @keyframes spin { to { transform: rotate(360deg); } }
    .message { color: var(--text-secondary); font-size: 0.9rem; }
  `],
})
export class LoadingSpinnerComponent {
  @Input() message?: string;
  @Input() overlay = false;
}
