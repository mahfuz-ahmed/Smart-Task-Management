import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { ApiResponse, AuthResponse, UserProfile } from '../models/auth.models';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7125/api/auth';

  private currentUserSubject = new BehaviorSubject<UserProfile | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();
  private router = inject(Router);

  constructor() {
    this.loadUserFromStorage();
  }

  private loadUserFromStorage() {
    const userJson = localStorage.getItem('stm_user');
    if (userJson) {
      try {
        this.currentUserSubject.next(JSON.parse(userJson));
      } catch {
        this.clearStorage();
      }
    }
  }

  get currentUserValue(): UserProfile | null {
    return this.currentUserSubject.value;
  }

  get token(): string | null {
    return localStorage.getItem('stm_access_token');
  }

  get refreshTokenValue(): string | null {
    return localStorage.getItem('stm_refresh_token');
  }

  register(data: any): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.apiUrl}/register`, data).pipe(
      tap(res => {
        if (res.success && res.data) {
          this.setSession(res.data);
        }
      })
    );
  }

  login(data: any): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.apiUrl}/login`, data).pipe(
      tap(res => {
        if (res.success && res.data) {
          this.setSession(res.data);
        }
      })
    );
  }

  refreshToken(): Observable<ApiResponse<AuthResponse>> {
    const body = {
      accessToken: this.token || '',
      refreshToken: this.refreshTokenValue || ''
    };
    return this.http.post<ApiResponse<AuthResponse>>(`${this.apiUrl}/refresh-token`, body).pipe(
      tap(res => {
        if (res.success && res.data) {
          this.setSession(res.data);
        } else {
          this.logout();
        }
      })
    );
  }

  logout(): Observable<any> {
    const rfToken = this.refreshTokenValue || '';
    // Send as object to match backend LogoutRequestDto
    return this.http.post(`${this.apiUrl}/logout`, { refreshToken: rfToken }).pipe(
      tap({
        finalize: () => {
          this.clearStorage();
          this.currentUserSubject.next(null);
        }
      })
    );
  }

  private setSession(auth: AuthResponse) {
    localStorage.setItem('stm_access_token', auth.accessToken);
    localStorage.setItem('stm_refresh_token', auth.refreshToken);
    localStorage.setItem('stm_user', JSON.stringify(auth.user));
    this.currentUserSubject.next(auth.user);
  }

  private clearStorage() {
    localStorage.removeItem('stm_access_token');
    localStorage.removeItem('stm_refresh_token');
    localStorage.removeItem('stm_user');
  }

  isLoggedIn(): boolean {
    return !!this.token;
  }

  hasRole(allowedRoles: string[]): boolean {
    const user = this.currentUserValue;
    if (!user) return false;
    return allowedRoles.includes(user.role);
  }

  forceLogout() {
    this.clearStorage();
    this.currentUserSubject.next(null);

    this.router.navigate(['/login']).catch(err => {
      console.error('Error navigating to login:', err);
    });
  }
}
