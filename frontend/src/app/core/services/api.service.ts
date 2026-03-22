import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ApiService {
  readonly base = 'http://localhost:5245/api';

  constructor(private http: HttpClient) {}

  get<T>(path: string, params?: Record<string, string | number>): Observable<T> {
    let p = new HttpParams();
    if (params) Object.entries(params).forEach(([k, v]) => p = p.set(k, v));
    return this.http.get<T>(`${this.base}/${path}`, { params: p });
  }

  post<T>(path: string, body?: unknown): Observable<T> {
    return this.http.post<T>(`${this.base}/${path}`, body);
  }

  put<T>(path: string, body?: unknown): Observable<T> {
    return this.http.put<T>(`${this.base}/${path}`, body);
  }

  delete<T>(path: string): Observable<T> {
    return this.http.delete<T>(`${this.base}/${path}`);
  }

  postForm<T>(path: string, form: FormData): Observable<T> {
    return this.http.post<T>(`${this.base}/${path}`, form);
  }
}
