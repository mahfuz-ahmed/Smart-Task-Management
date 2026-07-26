import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-toast-container',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="toast-container" aria-live="polite">
      @for (toast of toastService.toasts(); track toast.id) {
        <div class="toast toast-{{ toast.type }}" role="alert">
          <div class="toast-icon">
            @switch (toast.type) {
              @case ('success') { ✅ }
              @case ('error') { ❌ }
              @case ('warning') { ⚠️ }
              @case ('info') { ℹ️ }
            }
          </div>
          <div class="toast-content">
            <div class="toast-title">{{ toast.title }}</div>
            @if (toast.message) {
              <div class="toast-message">{{ toast.message }}</div>
            }
          </div>
          <button class="toast-close" (click)="toastService.remove(toast.id)" aria-label="Close">×</button>
        </div>
      }
    </div>
  `,
  styles: [`
    .toast-container {
      position: fixed;
      top: 20px;
      right: 20px;
      z-index: 5000;
      display: flex;
      flex-direction: column;
      gap: 8px;
      pointer-events: none;
      max-width: 380px;
    }
    .toast {
      display: flex;
      align-items: flex-start;
      gap: 12px;
      padding: 14px 16px;
      background: var(--color-bg-modal, #141c35);
      border: 1px solid var(--glass-border-strong, rgba(255,255,255,0.15));
      border-radius: 14px;
      box-shadow: 0 16px 48px rgba(0,0,0,0.5);
      pointer-events: auto;
      animation: toastIn 0.35s cubic-bezier(0.34, 1.56, 0.64, 1);
    }
    @keyframes toastIn {
      from { opacity: 0; transform: translateX(60px) scale(0.9); }
      to { opacity: 1; transform: translateX(0) scale(1); }
    }
    .toast-success { border-left: 3px solid #10b981; }
    .toast-error { border-left: 3px solid #ef4444; }
    .toast-warning { border-left: 3px solid #f59e0b; }
    .toast-info { border-left: 3px solid #3b82f6; }
    .toast-icon { font-size: 16px; margin-top: 2px; flex-shrink: 0; }
    .toast-content { flex: 1; min-width: 0; }
    .toast-title {
      font-weight: 700;
      font-size: 13px;
      color: var(--text-primary, rgba(255,255,255,0.95));
      margin-bottom: 2px;
    }
    .toast-message {
      font-size: 12px;
      color: var(--text-secondary, rgba(255,255,255,0.6));
      line-height: 1.4;
    }
    .toast-close {
      background: none;
      border: none;
      color: var(--text-muted, rgba(255,255,255,0.35));
      cursor: pointer;
      font-size: 18px;
      line-height: 1;
      padding: 0;
      flex-shrink: 0;
      transition: color 150ms;
    }
    .toast-close:hover { color: var(--text-primary, rgba(255,255,255,0.95)); }
  `]
})
export class ToastContainerComponent {
  toastService = inject(ToastService);
}
