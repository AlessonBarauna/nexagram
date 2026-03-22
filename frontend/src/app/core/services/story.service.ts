import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { Story, StoryFeedItem } from '../models';

@Injectable({ providedIn: 'root' })
export class StoryService {
  constructor(private api: ApiService) {}

  getFeed() {
    return this.api.get<StoryFeedItem[]>('stories/feed');
  }

  getById(id: string) {
    return this.api.get<Story>(`stories/${id}`);
  }

  create(form: FormData) {
    return this.api.postForm<Story>('stories', form);
  }

  delete(id: string) {
    return this.api.delete<void>(`stories/${id}`);
  }

  view(id: string) {
    return this.api.post<void>(`stories/${id}/view`);
  }

  getViewers(id: string) {
    return this.api.get<{ userId: string; username: string; avatarUrl: string | null; viewedAt: string }[]>(`stories/${id}/viewers`);
  }
}
