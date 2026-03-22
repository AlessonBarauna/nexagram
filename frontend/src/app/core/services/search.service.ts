import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { PagedResult, Post } from '../models';

interface SearchResult {
  users: { id: string; username: string; displayName: string; avatarUrl: string | null; followersCount: number }[];
  posts: Post[];
  hashtags: { name: string; postCount: number }[];
}

@Injectable({ providedIn: 'root' })
export class SearchService {
  constructor(private api: ApiService) {}

  search(query: string) {
    return this.api.get<SearchResult>('search', { q: query });
  }

  getPostsByHashtag(tag: string, page = 1, pageSize = 20) {
    return this.api.get<PagedResult<Post>>(`search/hashtag/${tag}`, { page, pageSize });
  }

  getTrending() {
    return this.api.get<{ name: string; postCount: number }[]>('search/trending');
  }
}
