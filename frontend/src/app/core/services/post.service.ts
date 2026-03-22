import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { Comment, PagedResult, Post } from '../models';

@Injectable({ providedIn: 'root' })
export class PostService {
  constructor(private api: ApiService) {}

  getById(id: string) {
    return this.api.get<Post>(`posts/${id}`);
  }

  create(data: { caption?: string; visibility: string; media: { key: string; mimeType: string }[] }) {
    return this.api.post<Post>('posts', data);
  }

  update(id: string, data: { caption?: string; visibility?: string }) {
    return this.api.put<Post>(`posts/${id}`, data);
  }

  delete(id: string) {
    return this.api.delete<void>(`posts/${id}`);
  }

  like(id: string) {
    return this.api.post<void>(`posts/${id}/like`);
  }

  unlike(id: string) {
    return this.api.delete<void>(`posts/${id}/like`);
  }

  save(id: string) {
    return this.api.post<void>(`posts/${id}/save`);
  }

  unsave(id: string) {
    return this.api.delete<void>(`posts/${id}/save`);
  }

  getComments(postId: string, page = 1, pageSize = 20) {
    return this.api.get<PagedResult<Comment>>(`posts/${postId}/comments`, { page, pageSize });
  }

  addComment(postId: string, text: string, parentCommentId?: string) {
    return this.api.post<Comment>(`posts/${postId}/comments`, { text, parentCommentId });
  }

  updateComment(commentId: string, text: string) {
    return this.api.put<Comment>(`comments/${commentId}`, { text });
  }

  deleteComment(commentId: string) {
    return this.api.delete<void>(`comments/${commentId}`);
  }

  likeComment(commentId: string) {
    return this.api.post<void>(`comments/${commentId}/like`);
  }

  unlikeComment(commentId: string) {
    return this.api.delete<void>(`comments/${commentId}/like`);
  }
}
