import { Component, inject, signal } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../core/services/auth.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, CommonModule],
  templateUrl: './shell.component.html',
  styleUrls: ['./shell.component.css']
})
export class ShellComponent {
  private authService = inject(AuthService);
  private toastService = inject(ToastService);
  private router = inject(Router);

  sidebarCollapsed = signal(false);

  user() {
    return this.authService.currentUserValue;
  }

  userInitials(): string {
    const u = this.authService.currentUserValue;
    if (!u) return '?';
    return `${u.firstName.charAt(0)}${u.lastName.charAt(0)}`.toUpperCase();
  }

  toggleSidebar() {
    this.sidebarCollapsed.update(v => !v);
  }

  onLogout() {
    this.authService.logout().subscribe({
      next: () => {
        this.toastService.success('Logged out', 'See you next time!');
        this.router.navigate(['/auth/login']);
      },
      error: () => {
        // Even on error, clear local storage and redirect
        this.router.navigate(['/auth/login']);
      }
    });
  }
}
