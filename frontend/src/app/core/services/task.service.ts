import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse } from '../models/auth.models';
import {
  TaskItem, CreateTaskRequest, UpdateTaskRequest,
  AddCommentRequest, PagedResult, TaskQueryParams
} from '../models/app.models';

@Injectable({ providedIn: 'root' })
export class TaskService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7125/api';

  getTasks(projectId: string, params?: TaskQueryParams): Observable<ApiResponse<PagedResult<TaskItem>>> {
    let httpParams = new HttpParams();
    if (params?.status) httpParams = httpParams.set('status', params.status);
    if (params?.priority) httpParams = httpParams.set('priority', params.priority);
    if (params?.search) httpParams = httpParams.set('search', params.search);
    if (params?.pageNumber) httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
    if (params?.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());

    return this.http.get<ApiResponse<PagedResult<TaskItem>>>(
      `${this.apiUrl}/projects/${projectId}/tasks`, { params: httpParams }
    );
  }

  getTask(projectId: string, taskId: string): Observable<ApiResponse<TaskItem>> {
    return this.http.get<ApiResponse<TaskItem>>(
      `${this.apiUrl}/projects/${projectId}/tasks/${taskId}`
    );
  }

  createTask(projectId: string, data: CreateTaskRequest): Observable<ApiResponse<TaskItem>> {
    return this.http.post<ApiResponse<TaskItem>>(
      `${this.apiUrl}/projects/${projectId}/tasks`, data
    );
  }

  updateTask(projectId: string, taskId: string, data: UpdateTaskRequest): Observable<ApiResponse<TaskItem>> {
    return this.http.put<ApiResponse<TaskItem>>(
      `${this.apiUrl}/projects/${projectId}/tasks/${taskId}`, data
    );
  }

  deleteTask(projectId: string, taskId: string): Observable<ApiResponse<any>> {
    return this.http.delete<ApiResponse<any>>(
      `${this.apiUrl}/projects/${projectId}/tasks/${taskId}`
    );
  }

  updateStatus(projectId: string, taskId: string, status: number): Observable<ApiResponse<TaskItem>> {
    return this.http.patch<ApiResponse<TaskItem>>(
      `${this.apiUrl}/projects/${projectId}/tasks/${taskId}/status`, { status }
    );
  }

  addComment(projectId: string, taskId: string, data: AddCommentRequest): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(
      `${this.apiUrl}/projects/${projectId}/tasks/${taskId}/comments`, data
    );
  }

  deleteComment(projectId: string, taskId: string, commentId: string): Observable<ApiResponse<any>> {
    return this.http.delete<ApiResponse<any>>(
      `${this.apiUrl}/projects/${projectId}/tasks/${taskId}/comments/${commentId}`
    );
  }

  // My tasks across all projects
  getMyTasks(params?: TaskQueryParams): Observable<ApiResponse<PagedResult<TaskItem>>> {
    let httpParams = new HttpParams();
    if (params?.status) httpParams = httpParams.set('status', params.status);
    if (params?.priority) httpParams = httpParams.set('priority', params.priority);
    if (params?.search) httpParams = httpParams.set('search', params.search);
    if (params?.pageNumber) httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
    if (params?.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());

    return this.http.get<ApiResponse<PagedResult<TaskItem>>>(
      `${this.apiUrl}/tasks/my-tasks`, { params: httpParams }
    );
  }
}
