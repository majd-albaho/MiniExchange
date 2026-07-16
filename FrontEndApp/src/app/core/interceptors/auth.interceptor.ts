import {
  HttpInterceptorFn,
  HttpRequest,
  HttpErrorResponse,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, from, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

const withToken = (req: HttpRequest<unknown>, token: string): HttpRequest<unknown> =>
  req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });

/** Login/refresh calls must never trigger a refresh themselves, or a failing refresh recurses. */
const isAuthEndpoint = (url: string): boolean => url.includes('/Auth/');

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();

  return next(token ? withToken(req, token) : req).pipe(
    catchError((error: unknown) => {
      const expired =
        error instanceof HttpErrorResponse &&
        error.status === 401 &&
        !!token &&
        !isAuthEndpoint(req.url);

      if (!expired) {
        return throwError(() => error);
      }

      // The token was rejected — refresh once (shared across concurrent 401s) and replay.
      return from(authService.refreshTokenOnce()).pipe(
        switchMap(newToken =>
          newToken ? next(withToken(req, newToken)) : throwError(() => error)
        )
      );
    })
  );
};
