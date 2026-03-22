import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { Notification, PagedResult } from '../models';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  constructor(private api: ApiService) {}

  getAll(page = 1, pageSize = 20) {
    return this.api.get<PagedResult<Notification>>('notifications', { page, pageSize });
  }

  getUnreadCount() {
    return this.api.get<{ count: number }>('notifications/unread-count');
  }

  markRead(id: string) {
    return this.api.post<void>(`notifications/${id}/read`);
  }

  markAllRead() {
    return this.api.post<void>('notifications/read-all');
  }
}
