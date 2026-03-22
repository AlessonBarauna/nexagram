import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { UserSummary } from '../models';

@Injectable({ providedIn: 'root' })
export class FollowService {
  constructor(private api: ApiService) {}

  follow(username: string) {
    return this.api.post<void>(`follow/${username}`);
  }

  unfollow(username: string) {
    return this.api.delete<void>(`follow/${username}`);
  }

  isFollowing(username: string) {
    return this.api.get<{ isFollowing: boolean }>(`follow/${username}/is-following`);
  }

  getSuggestions() {
    return this.api.get<UserSummary[]>('follow/suggestions');
  }
}
