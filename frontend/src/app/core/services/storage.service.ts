import { Injectable } from '@angular/core';
import { forkJoin, Observable, switchMap, of } from 'rxjs';
import { ApiService } from './api.service';
import { HttpClient } from '@angular/common/http';

export interface UploadedMedia {
  key: string;
  mimeType: string;
}

interface UploadResult {
  key: string;
  url: string;
  mimeType: string;
}

@Injectable({ providedIn: 'root' })
export class StorageService {
  constructor(private api: ApiService, private http: HttpClient) {}

  uploadFiles(files: File[]): Observable<UploadedMedia[]> {
    if (!files.length) return of([]);
    return forkJoin(files.map(f => this.uploadOne(f)));
  }

  private uploadOne(file: File): Observable<UploadedMedia> {
    const form = new FormData();
    form.append('file', file);
    return this.api.postForm<UploadResult>('storage/upload', form);
  }
}
