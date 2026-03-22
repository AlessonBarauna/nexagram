import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { Conversation, DirectMessage, PagedResult } from '../models';

@Injectable({ providedIn: 'root' })
export class DmService {
  constructor(private api: ApiService) {}

  getConversations() {
    return this.api.get<Conversation[]>('dm/conversations');
  }

  getMessages(conversationId: string, page = 1, pageSize = 30) {
    return this.api.get<PagedResult<DirectMessage>>(`dm/conversations/${conversationId}/messages`, { page, pageSize });
  }

  send(recipientUsername: string, text: string, ephemeralSeconds?: number) {
    return this.api.post<DirectMessage>('dm', { recipientUsername, text, ephemeralSeconds });
  }

  markRead(conversationId: string) {
    return this.api.post<void>(`dm/conversations/${conversationId}/read`);
  }

  deleteMessage(messageId: string) {
    return this.api.delete<void>(`dm/${messageId}`);
  }
}
