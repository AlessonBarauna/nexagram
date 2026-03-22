import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { PagedResult, Post, Story, UserProfile, UserSummary } from '../models';

@Injectable({ providedIn: 'root' })
export class UserService {
  constructor(private api: ApiService) {}

  getProfile(username: string) {
    return this.api.get<UserProfile>(`users/${username}`);
  }

  updateProfile(data: { displayName?: string; bio?: string; website?: string; isPrivate?: boolean }) {
    return this.api.put<UserProfile>('users/me', data);
  }

  uploadAvatar(file: File) {
    const form = new FormData();
    form.append('file', file);
    return this.api.postForm<{ avatarUrl: string }>('users/me/avatar', form);
  }

  getFollowers(username: string, page = 1, pageSize = 20) {
    return this.api.get<PagedResult<UserSummary>>(`users/${username}/followers`, { page, pageSize });
  }

  getFollowing(username: string, page = 1, pageSize = 20) {
    return this.api.get<PagedResult<UserSummary>>(`users/${username}/following`, { page, pageSize });
  }

  getUserPosts(username: string, page = 1, pageSize = 12) {
    return this.api.get<PagedResult<Post>>(`users/${username}/posts`, { page, pageSize });
  }

  getUserStories(username: string) {
    return this.api.get<Story[]>(`users/${username}/stories`);
  }
}
