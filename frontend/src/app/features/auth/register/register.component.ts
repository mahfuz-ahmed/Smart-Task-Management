import { Component, inject, signal } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators,
  ReactiveFormsModule,
  AbstractControl,
  ValidationErrors,
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';

function passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
  const password = control.get('password');
  const confirmPassword = control.get('confirmPassword');
  if (password && confirmPassword && password.value !== confirmPassword.value) {
    return { passwordMismatch: true };
  }
  return null;
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css',
})
export class RegisterComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private toastService = inject(ToastService);

  loading = signal(false);
  errorMessage = signal('');
  showPassword = signal(false);

  // Role options for dropdown
  roleOptions = [
    { value: 1, label: 'Admin', description: 'Can everything' },
    { value: 2, label: 'Project Manager', description: 'Can create and manage projects' },
    { value: 3, label: 'Team Member', description: 'Can work on assigned tasks' },
  ];

  form: FormGroup = this.fb.group(
    {
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', Validators.required],
      role: [3, Validators.required], // Default to TeamMember (3)
    },
    { validators: passwordMatchValidator },
  );

  isInvalid(field: string): boolean {
    const control = this.form.get(field);
    return !!(control?.invalid && control?.touched);
  }

  togglePassword() {
    this.showPassword.update((v) => !v);
  }

  passwordStrength(): number {
    const pw = this.form.get('password')?.value || '';
    let score = 0;
    if (pw.length >= 8) score++;
    if (/[A-Z]/.test(pw) || /[0-9]/.test(pw)) score++;
    if (/[!@#$%^&*(),.?":{}|<>]/.test(pw)) score++;
    return score;
  }

  strengthLabel(): string {
    const s = this.passwordStrength();
    return s === 1 ? 'Weak' : s === 2 ? 'Fair' : s === 3 ? 'Strong' : '';
  }

  onSubmit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.errorMessage.set('');

    const { confirmPassword, ...payload } = this.form.value;

    this.authService.register(payload).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success) {
          this.toastService.success(
            'Account created!',
            `Welcome to SmartTask, ${res.data.user.firstName}!`,
          );
          this.router.navigate(['/dashboard']);
        } else {
          this.errorMessage.set(res.message || 'Registration failed. Please try again.');
        }
      },
      error: (err) => {
        this.loading.set(false);
        const msgs = err?.error?.errors;
        if (Array.isArray(msgs) && msgs.length > 0) {
          this.errorMessage.set(msgs[0]);
        } else {
          this.errorMessage.set(err?.error?.message || 'Registration failed. Please try again.');
        }
      },
    });
  }
}
