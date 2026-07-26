import { Component, inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';

interface DialogData {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
}

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule],
  template: `
    <h2 class="dialog-header" mat-dialog-title>{{ data.title }}</h2>
    <mat-dialog-content class="dialog-content">
      <p>{{ data.message }}</p>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="dialogRef.close(false)">{{ data.cancelText || 'Cancel' }}</button>
      <button mat-button color="primary" (click)="dialogRef.close(true)">{{ data.confirmText || 'Confirm' }}</button>
    </mat-dialog-actions>
  `,
  styles: [
    `
      ::ng-deep .mat-dialog-container {
        background: var(--color-bg-modal);
        border: 1px solid var(--glass-border-strong);
        border-radius: var(--radius-2xl);
        box-shadow: var(--shadow-xl);
        backdrop-filter: var(--glass-blur);
      }
      .dialog-header {
        background: var(--brand-gradient);
        color: var(--text-primary);
        padding: var(--space-4);
        border-top-left-radius: var(--radius-2xl);
        border-top-right-radius: var(--radius-2xl);
      }
      .dialog-content {
        padding: var(--space-4);
      }
    `
  ]
})
export class ConfirmDialogComponent {
  dialogRef = inject(MatDialogRef<ConfirmDialogComponent>);
  data = inject(MAT_DIALOG_DATA) as DialogData;
}
