import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { ApiResponse } from '../models/auth.models';

export interface EnhanceRequest {
  description: string;
  context?: string;
}

export interface EnhanceResponse {
  improvedDescription: string;
}

@Injectable({ providedIn: 'root' })
export class AiService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7125/api/ai';

  /**
   * Calls backend AI endpoint to improve task description.
   * Falls back to a simple client-side enhancement if API is unavailable.
   */
  enhanceDescription(req: EnhanceRequest): Observable<string> {
    const payload = { description: req.description, taskTitle: req.context };
    return this.http
      .post<
        ApiResponse<{
          improvedDescription?: string;
          ImprovedDescription?: string;
          improved_description?: string;
        }>
      >(`${this.apiUrl}/improve-description`, payload)
      .pipe(
        map((res) => {
          const improved =
            res.data?.improvedDescription ||
            res.data?.ImprovedDescription ||
            res.data?.improved_description ||
            req.description;
          return improved?.trim() ? improved : this.basicEnhance(req.description);
        }),
        catchError(() => {
          const improved = this.basicEnhance(req.description);
          return of(improved);
        }),
      );
  }

  enhanceProjectDescription(req: EnhanceRequest): Observable<string> {
    const payload = { description: req.description, taskTitle: req.context };
    return this.http
      .post<
        ApiResponse<{
          improvedDescription?: string;
          ImprovedDescription?: string;
          improved_description?: string;
        }>
      >(`${this.apiUrl}/enhance-project-description`, payload)
      .pipe(
        map((res) => {
          const improved =
            res.data?.improvedDescription ||
            res.data?.ImprovedDescription ||
            res.data?.improved_description ||
            req.description;
          return improved?.trim() ? improved : this.basicEnhance(req.description);
        }),
        catchError(() => {
          const improved = this.basicEnhance(req.description);
          return of(improved);
        }),
      );
  }

  /**
   * Simple client-side text improvement as fallback
   */
  private basicEnhance(text: string): string {
    if (!text || text.trim().length === 0) return text;

    let result = text.trim().replace(/\s+/g, ' ');
    result = result.charAt(0).toUpperCase() + result.slice(1);

    if (!/[.!?]$/.test(result)) {
      result += '.';
    }

    result = result.replace(/\.\s+([a-z])/g, (_match, letter) => '. ' + letter.toUpperCase());

    const words = result.split(/\s+/).filter(Boolean);
    if (words.length <= 8) {
      const clean = result.replace(/[.!?]$/, '').trim();
      result = `Implement and complete ${clean.toLowerCase()}`;
      if (!/[.!?]$/.test(result)) result += '.';
    }

    return result;
  }
}
