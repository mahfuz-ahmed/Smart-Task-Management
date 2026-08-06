import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse } from '../models/auth.models';
import { UserProfile } from '../models/auth.models';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/users`;

  /**
   * Search users excluding members of a given project.
   * @param term Search term (minimum 2 characters). Empty or short term returns empty list.
   * @param excludeProjectId Project Id to exclude existing members.
   * @param limit Maximum number of results (default 10, max 20).
   */
  search(term: string, excludeProjectId: string, limit: number = 10): Observable<ApiResponse<UserProfile[]>> {
    // Enforce minimum term length as per backend contract
    if (!term || term.trim().length < 2) {
      // Return empty observable synchronously
      return new Observable(observer => {
        observer.next({ success: true, message: '', data: [], errors: [] } as ApiResponse<UserProfile[]>);
        observer.complete();
      });
    }
    const params = new HttpParams()
      .set('term', term.trim())
      .set('excludeProjectId', excludeProjectId)
      .set('limit', Math.min(limit, 20).toString());
    return this.http.get<ApiResponse<UserProfile[]>>(`${this.apiUrl}/search`, { params });
  }
}
