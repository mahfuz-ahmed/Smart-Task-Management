import { Injectable, signal } from '@angular/core';

export interface Toast {
  id: string;
  type: 'success' | 'error' | 'warning' | 'info';
  title: string;
  message?: string;
  duration?: number;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  toasts = signal<Toast[]>([]);

  private add(toast: Omit<Toast, 'id'>) {
    const id = Math.random().toString(36).slice(2);
    this.toasts.update(ts => [...ts, { ...toast, id }]);
    setTimeout(() => this.remove(id), toast.duration ?? 4000);
  }

  remove(id: string) {
    this.toasts.update(ts => ts.filter(t => t.id !== id));
  }

  success(title: string, message?: string) {
    this.add({ type: 'success', title, message });
  }

  error(title: string, message?: string) {
    this.add({ type: 'error', title, message, duration: 6000 });
  }

  warning(title: string, message?: string) {
    this.add({ type: 'warning', title, message });
  }

  info(title: string, message?: string) {
    this.add({ type: 'info', title, message });
  }
}
