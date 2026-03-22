import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { NotificationService } from '../../core/services/notification.service';
import { Notification } from '../../core/models';
import { AvatarModule } from 'primeng/avatar';
import { ButtonModule } from 'primeng/button';
import { SkeletonModule } from 'primeng/skeleton';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [CommonModule, RouterLink, AvatarModule, ButtonModule, SkeletonModule],
  templateUrl: './notifications.component.html',
  styleUrl: './notifications.component.scss'
})
export class NotificationsComponent implements OnInit {
  private notifService = inject(NotificationService);

  notifications = signal<Notification[]>([]);
  loading = signal(true);

  ngOnInit() {
    this.notifService.getAll().subscribe({
      next: (r) => { this.notifications.set(r.items); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  markAllRead() {
    this.notifService.markAllRead().subscribe(() => {
      this.notifications.update(list => list.map(n => ({ ...n, isRead: true })));
    });
  }

  markRead(n: Notification) {
    if (n.isRead) return;
    this.notifService.markRead(n.id).subscribe(() => {
      this.notifications.update(list => list.map(x => x.id === n.id ? { ...x, isRead: true } : x));
    });
  }

  notifLabel(n: Notification): string {
    switch (n.type) {
      case 'Follow': return 'começou a te seguir';
      case 'Like': return 'curtiu seu post';
      case 'Comment': return 'comentou no seu post';
      case 'Mention': return 'te mencionou';
      case 'StoryView': return 'viu sua story';
      default: return '';
    }
  }
}
