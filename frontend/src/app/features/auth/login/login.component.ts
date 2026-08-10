import { Component, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private toastService = inject(ToastService);

  loading = signal(false);
  errorMessage = signal('');
  showPassword = signal(false);

  form: FormGroup = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
  });

  isInvalid(field: string): boolean {
    const control = this.form.get(field);
    return !!(control?.invalid && control?.touched);
  }

  togglePassword() {
    this.showPassword.update((v) => !v);
  }

  onSubmit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.errorMessage.set('');

    this.authService.login(this.form.value).subscribe({
      next: (res) => {
        this.loading.set(false);

        if (res.success) {
          this.toastService.success('Welcome back!', `Hello, ${res.data.user.firstName}`);

          this.router.navigate(['/dashboard']);
        } else {
          this.errorMessage.set(res.message || 'Login failed. Please try again.');
        }
      },

      error: (err) => {
        this.loading.set(false);

        const errors = err?.error?.errors;

        const msg =
          Array.isArray(errors) && errors.length > 0
            ? errors[0]
            : err?.error?.message || 'Login failed. Please check your credentials.';

        this.errorMessage.set(msg);
      },
    });
  }
}
