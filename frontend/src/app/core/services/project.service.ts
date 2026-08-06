import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse } from '../models/auth.models';
import {
  Project,
  CreateProjectRequest,
  UpdateProjectRequest,
  PagedResult,
  AddMemberRequest,
  ProjectMember,
  ProjectQueryParams,
} from '../models/app.models';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ProjectService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/projects`;

  getProjects(params?: ProjectQueryParams): Observable<ApiResponse<PagedResult<Project>>> {
    let httpParams = new HttpParams();
    if (params?.search) httpParams = httpParams.set('search', params.search);
    if (params?.status !== undefined)
      httpParams = httpParams.set('status', params.status.toString());
    if (params?.priority !== undefined)
      httpParams = httpParams.set('priority', params.priority.toString());
    if (params?.page) httpParams = httpParams.set('page', params.page.toString());
    if (params?.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
    return this.http.get<ApiResponse<PagedResult<Project>>>(this.apiUrl, { params: httpParams });
  }

  getProject(id: string): Observable<ApiResponse<Project>> {
    return this.http.get<ApiResponse<Project>>(`${this.apiUrl}/${id}`);
  }

  createProject(data: CreateProjectRequest): Observable<ApiResponse<Project>> {
    return this.http.post<ApiResponse<Project>>(this.apiUrl, data);
  }

  updateProject(id: string, data: UpdateProjectRequest): Observable<ApiResponse<Project>> {
    return this.http.put<ApiResponse<Project>>(`${this.apiUrl}/${id}`, data);
  }

  deleteProject(id: string): Observable<ApiResponse<any>> {
    return this.http.delete<ApiResponse<any>>(`${this.apiUrl}/${id}`);
  }

  getMembers(projectId: string): Observable<ApiResponse<ProjectMember[]>> {
    return this.http.get<ApiResponse<ProjectMember[]>>(`${this.apiUrl}/${projectId}/members`);
  }

  addMember(projectId: string, data: AddMemberRequest): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/${projectId}/members`, data);
  }

  removeMember(projectId: string, userId: string): Observable<ApiResponse<any>> {
    return this.http.delete<ApiResponse<any>>(`${this.apiUrl}/${projectId}/members/${userId}`);
  }
}
