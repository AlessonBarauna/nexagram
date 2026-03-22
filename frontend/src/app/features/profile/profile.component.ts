import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { UserService } from '../../core/services/user.service';
import { FollowService } from '../../core/services/follow.service';
import { AuthService } from '../../core/services/auth.service';
import { Post, UserProfile } from '../../core/models';
import { AvatarModule } from 'primeng/avatar';
import { ButtonModule } from 'primeng/button';
import { TabViewModule } from 'primeng/tabview';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { SkeletonModule } from 'primeng/skeleton';
import { MessageService } from 'primeng/api';
import { PostCardComponent } from '../../shared/post-card/post-card.component';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    CommonModule, RouterLink, FormsModule,
    AvatarModule, ButtonModule, TabViewModule, DialogModule,
    InputTextModule, InputTextareaModule, SkeletonModule, PostCardComponent
  ],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss'
})
export class ProfileComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private userService = inject(UserService);
  private followService = inject(FollowService);
  private auth = inject(AuthService);
  private messageService = inject(MessageService);

  profile = signal<UserProfile | null>(null);
  posts = signal<Post[]>([]);
  loading = signal(true);
  isFollowing = signal(false);
  followLoading = signal(false);

  activeTab = signal<'posts' | 'saved'>('posts');
  editDialogVisible = signal(false);
  editDisplayName = '';
  editBio = '';
  editWebsite = '';
  editSaving = signal(false);

  get isOwner() {
    return this.auth.user()?.username === this.profile()?.username;
  }

  ngOnInit() {
    this.route.params.subscribe(p => this.loadProfile(p['username']));
  }

  loadProfile(username: string) {
    this.loading.set(true);
    this.userService.getProfile(username).subscribe({
      next: (p) => {
        this.profile.set(p);
        this.loading.set(false);
        this.loadPosts(username);
        if (!this.isOwner) this.checkFollowing(username);
      },
      error: () => this.loading.set(false)
    });
  }

  loadPosts(username: string) {
    this.userService.getUserPosts(username).subscribe(r => this.posts.set(r.items));
  }

  checkFollowing(username: string) {
    this.followService.isFollowing(username).subscribe(r => this.isFollowing.set(r.isFollowing));
  }

  toggleFollow() {
    const p = this.profile();
    if (!p) return;
    this.followLoading.set(true);
    const obs = this.isFollowing()
      ? this.followService.unfollow(p.username)
      : this.followService.follow(p.username);

    obs.subscribe({
      next: () => {
        this.isFollowing.update(v => !v);
        this.profile.update(u => u ? {
          ...u,
          followerCount: u.followerCount + (this.isFollowing() ? 1 : -1)
        } : u);
        this.followLoading.set(false);
      },
      error: () => this.followLoading.set(false)
    });
  }

  openEditDialog() {
    const p = this.profile()!;
    this.editDisplayName = p.displayName ?? '';
    this.editBio = p.bio ?? '';
    this.editWebsite = p.website ?? '';
    this.editDialogVisible.set(true);
  }

  saveProfile() {
    this.editSaving.set(true);
    this.userService.updateProfile({
      displayName: this.editDisplayName,
      bio: this.editBio,
      website: this.editWebsite
    }).subscribe({
      next: (updated) => {
        this.profile.set(updated);
        this.editDialogVisible.set(false);
        this.editSaving.set(false);
        this.messageService.add({ severity: 'success', summary: 'Perfil atualizado' });
      },
      error: () => this.editSaving.set(false)
    });
  }

  onPostDeleted(id: string) {
    this.posts.update(p => p.filter(x => x.id !== id));
  }
}
