import { Component, inject, OnInit, signal, ViewChild } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';
import { Post } from '../../core/models';
import { AvatarModule } from 'primeng/avatar';
import { BadgeModule } from 'primeng/badge';
import { TooltipModule } from 'primeng/tooltip';
import { ToastModule } from 'primeng/toast';
import { PostCreateComponent } from '../../shared/post-create/post-create.component';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [
    CommonModule, RouterOutlet, RouterLink, RouterLinkActive,
    AvatarModule, BadgeModule, TooltipModule, ToastModule,
    PostCreateComponent
  ],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.scss'
})
export class ShellComponent implements OnInit {
  @ViewChild(PostCreateComponent) postCreate!: PostCreateComponent;

  private auth = inject(AuthService);
  private notifService = inject(NotificationService);
  private router = inject(Router);

  user = this.auth.user;
  unreadCount = signal(0);
  collapsed = signal(false);

  navItems = [
    { icon: 'pi pi-home', label: 'Feed', route: '/feed' },
    { icon: 'pi pi-compass', label: 'Explorar', route: '/explore' },
    { icon: 'pi pi-send', label: 'Mensagens', route: '/messages' },
    { icon: 'pi pi-bell', label: 'Notificações', route: '/notifications' },
    { icon: 'pi pi-search', label: 'Pesquisar', route: '/search' },
  ];

  ngOnInit() {
    this.loadUnreadCount();
  }

  loadUnreadCount() {
    this.notifService.getUnreadCount().subscribe(r => this.unreadCount.set(r.count));
  }

  goToProfile() {
    const u = this.user();
    if (u) this.router.navigate(['/profile', u.username]);
  }

  logout() { this.auth.logout(); }

  toggleSidebar() { this.collapsed.update(v => !v); }

  openCreate() { this.postCreate.open(); }

  onPostCreated(post: Post) {
    if (this.router.url === '/feed') {
      window.dispatchEvent(new CustomEvent('post-created', { detail: post }));
    }
  }
}
