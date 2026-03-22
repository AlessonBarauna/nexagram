import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DmService } from '../../core/services/dm.service';
import { AuthService } from '../../core/services/auth.service';
import { Conversation, DirectMessage } from '../../core/models';
import { AvatarModule } from 'primeng/avatar';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SkeletonModule } from 'primeng/skeleton';

@Component({
  selector: 'app-messages',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, AvatarModule, ButtonModule, InputTextModule, SkeletonModule],
  templateUrl: './messages.component.html',
  styleUrl: './messages.component.scss'
})
export class MessagesComponent implements OnInit {
  private dmService = inject(DmService);
  private auth = inject(AuthService);
  private route = inject(ActivatedRoute);

  conversations = signal<Conversation[]>([]);
  activeConversation = signal<Conversation | null>(null);
  messages = signal<DirectMessage[]>([]);
  messageText = '';
  loadingConvs = signal(true);
  loadingMsgs = signal(false);

  currentUser = this.auth.user;

  ngOnInit() {
    this.loadConversations();
    this.route.params.subscribe(p => {
      if (p['username']) {
        // Open conversation with specific user after loading
      }
    });
  }

  loadConversations() {
    this.dmService.getConversations().subscribe({
      next: (c) => { this.conversations.set(c); this.loadingConvs.set(false); },
      error: () => this.loadingConvs.set(false)
    });
  }

  openConversation(conv: Conversation) {
    this.activeConversation.set(conv);
    this.loadingMsgs.set(true);
    this.dmService.getMessages(conv.participant.id).subscribe({
      next: (r) => { this.messages.set(r.items.reverse()); this.loadingMsgs.set(false); },
      error: () => this.loadingMsgs.set(false)
    });
    if (conv.unreadCount > 0) {
      this.dmService.markRead(conv.participant.id).subscribe();
    }
  }

  sendMessage() {
    const conv = this.activeConversation();
    if (!this.messageText.trim() || !conv) return;
    const text = this.messageText;
    this.messageText = '';
    this.dmService.send(conv.participant.username, text).subscribe(msg => {
      this.messages.update(list => [...list, msg]);
    });
  }

  isMe(msg: DirectMessage): boolean {
    return msg.sender.id === this.currentUser()?.id;
  }
}
