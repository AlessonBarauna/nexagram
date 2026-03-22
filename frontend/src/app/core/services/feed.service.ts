import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { PagedResult, Post } from '../models';

@Injectable({ providedIn: 'root' })
export class FeedService {
  constructor(private api: ApiService) {}

  getPersonal(page = 1, pageSize = 20) {
    return this.api.get<PagedResult<Post>>('feed', { page, pageSize });
  }

  getExplore(page = 1, pageSize = 20) {
    return this.api.get<PagedResult<Post>>('feed/explore', { page, pageSize });
  }
}
