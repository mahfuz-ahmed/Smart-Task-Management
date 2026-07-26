
// import { HttpInterceptorFn, HttpErrorResponse, HttpRequest, HttpHandlerFn, HttpEvent } from '@angular/common/http';
// import { inject } from '@angular/core';
// import { AuthService } from '../services/auth.service';
// import { catchError, switchMap, throwError, BehaviorSubject, filter, take, Observable } from 'rxjs';

// let isRefreshing = false;
// const refreshTokenSubject = new BehaviorSubject<string | null>(null);

// export const authInterceptor: HttpInterceptorFn = (req, next) => {
//   const authService = inject(AuthService);
//   const token = authService.token;

//   let authReq = req;
//   if (token && !req.url.includes('/api/auth/refresh-token')) {
//     authReq = req.clone({
//       setHeaders: {
//         Authorization: `Bearer ${token}`
//       }
//     });
//   }

//   return next(authReq).pipe(
//     catchError((error) => {
//       if (error instanceof HttpErrorResponse) {
//         if (error.status === 0) {
//           console.error('Backend is not responding:', error.message);
//           authService.forceLogout();
//           return throwError(() => new Error('Backend server is not responding'));
//         }

//         if (
//           error.status === 401 &&
//           !req.url.includes('/api/auth/login') &&
//           !req.url.includes('/api/auth/register') &&
//           !req.url.includes('/api/auth/refresh-token')
//         ) {
//           return handle401Error(req, next, authService);
//         }
//       }
//       console.error('❌ HTTP Error:', {
//         status: error?.status,
//         message: error?.message,
//         url: req.url
//       });

//       return throwError(() => error);
//     })
//   );
// };

// function handle401Error(
//   req: HttpRequest<any>,
//   next: HttpHandlerFn,
//   authService: AuthService
// ): Observable<HttpEvent<any>> {

//   if (!isRefreshing) {
//     isRefreshing = true;
//     refreshTokenSubject.next(null);

//     return authService.refreshToken().pipe(
//       switchMap((res) => {
//         if (!res || !res.data) {
//           throw new Error('Invalid refresh token response');
//         }
//         isRefreshing = false;
//         refreshTokenSubject.next(res.data.accessToken);
//         return next(req.clone({
//           setHeaders: {
//             Authorization: `Bearer ${res.data.accessToken}`
//           }
//         }));
//       }),
//       catchError((err) => {
//         isRefreshing = false;
//         authService.forceLogout();
//         return throwError(() => new Error('Session expired. Please login again.'));
//       })
//     );
//   } else {
//     return refreshTokenSubject.pipe(
//       filter((token): token is string => token !== null),
//       take(1),
//       switchMap((token) => {
//         return next(req.clone({
//           setHeaders: {
//             Authorization: `Bearer ${token}`
//           }
//         }));
//       })
//     );
//   }
// }

import { HttpInterceptorFn, HttpErrorResponse, HttpRequest, HttpHandlerFn, HttpEvent } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { catchError, switchMap, throwError, BehaviorSubject, filter, take, Observable } from 'rxjs';

let isRefreshing = false;
const refreshTokenSubject = new BehaviorSubject<string | null>(null);

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.token; // Access token getter

  let authReq = req;

  // Don't attach Auth header for Auth endpoints
  if (token && !isAuthEndpoint(req.url)) {
    authReq = addTokenHeader(req, token);
  }

  return next(authReq).pipe(
    catchError((error) => {
      if (error instanceof HttpErrorResponse) {
        // 1. Backend Server Down (Status 0 or CORS issue)
        if (error.status === 0) {
          console.error('Backend server is down or unreachable:', error.message);
          authService.forceLogout();
          return throwError(() => new Error('Backend server is not responding.'));
        }

        // 2. Handling 401 Unauthorized
        if (error.status === 401 && !isAuthEndpoint(req.url)) {
          return handle401Error(req, next, authService);
        }
      }

      console.error('HTTP Error:', {
        status: error?.status,
        message: error?.message,
        url: req.url
      });

      return throwError(() => error);
    })
  );
};

function handle401Error(
  req: HttpRequest<unknown>,
  next: HttpHandlerFn,
  authService: AuthService
): Observable<HttpEvent<unknown>> {

  const accessToken = authService.token;
  const refreshToken = authService.refreshTokenValue; // Getter for Refresh Token

  // SAFETY GUARD: Token Missing -> Directly logout without requesting refresh
  if (!accessToken || !refreshToken) {
    console.warn('Tokens are missing. Redirecting to login.');
    authService.forceLogout();
    return throwError(() => new Error('Authentication tokens missing.'));
  }

  if (!isRefreshing) {
    isRefreshing = true;
    refreshTokenSubject.next(null);

    return authService.refreshToken().pipe(
      switchMap((res) => {
        isRefreshing = false;

        // Type-safe access to response payload
        const newAccessToken = res?.data?.accessToken;

        if (!newAccessToken) {
          authService.forceLogout();
          return throwError(() => new Error('Invalid refresh response from server'));
        }

        refreshTokenSubject.next(newAccessToken);
        return next(addTokenHeader(req, newAccessToken));
      }),
      catchError((err) => {
        isRefreshing = false;
        authService.forceLogout();
        return throwError(() => new Error('Session expired. Please login again.'));
      })
    );
  } else {
    // Queue subsequent failing 401 requests until refresh finishes
    return refreshTokenSubject.pipe(
      filter((token): token is string => token !== null),
      take(1),
      switchMap((token) => next(addTokenHeader(req, token)))
    );
  }
}

// ── Helper Functions ─────────────────────────────────────────

function addTokenHeader(req: HttpRequest<unknown>, token: string): HttpRequest<unknown> {
  return req.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`
    }
  });
}

function isAuthEndpoint(url: string): boolean {
  return (
    url.includes('/api/auth/login') ||
    url.includes('/api/auth/register') ||
    url.includes('/api/auth/refresh-token')
  );
}