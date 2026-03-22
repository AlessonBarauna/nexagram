import { Component, inject, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FeedService } from '../../core/services/feed.service';
import { StoryService } from '../../core/services/story.service';
import { FollowService } from '../../core/services/follow.service';
import { Post, StoryFeedItem, UserSummary } from '../../core/models';
import { PostCardComponent } from '../../shared/post-card/post-card.component';
import { StoriesBarComponent } from '../../shared/stories-bar/stories-bar.component';
import { RouterLink } from '@angular/router';
import { AvatarModule } from 'primeng/avatar';
import { ButtonModule } from 'primeng/button';
import { SkeletonModule } from 'primeng/skeleton';

@Component({
  selector: 'app-feed',
  standalone: true,
  imports: [CommonModule, PostCardComponent, StoriesBarComponent, RouterLink, AvatarModule, ButtonModule, SkeletonModule],
  templateUrl: './feed.component.html',
  styleUrl: './feed.component.scss'
})
export class FeedComponent implements OnInit, OnDestroy {
  private feedService = inject(FeedService);
  private storyService = inject(StoryService);
  private followService = inject(FollowService);

  posts = signal<Post[]>([]);
  stories = signal<StoryFeedItem[]>([]);
  suggestions = signal<UserSummary[]>([]);
  loading = signal(true);
  hasNewPost = signal(false);
  page = 1;
  hasMore = signal(true);

  private postCreatedHandler = (e: Event) => {
    const post = (e as CustomEvent<Post>).detail;
    this.posts.update(p => [post, ...p]);
    this.hasNewPost.set(true);
    setTimeout(() => this.hasNewPost.set(false), 3000);
  };

  ngOnInit() {
    this.loadStories();
    this.loadPosts();
    this.loadSuggestions();
    window.addEventListener('post-created', this.postCreatedHandler);
  }

  ngOnDestroy() {
    window.removeEventListener('post-created', this.postCreatedHandler);
  }

  loadStories() {
    this.storyService.getFeed().subscribe(data => this.stories.set(data));
  }

  loadPosts(reset = false) {
    if (reset) { this.page = 1; this.posts.set([]); }
    this.loading.set(true);
    this.feedService.getPersonal(this.page).subscribe({
      next: (r) => {
        this.posts.update(p => [...p, ...r.items]);
        this.hasMore.set(r.hasNext);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  loadMore() {
    if (!this.hasMore()) return;
    this.page++;
    this.loadPosts();
  }

  loadSuggestions() {
    this.followService.getSuggestions().subscribe(data => this.suggestions.set(data.slice(0, 5)));
  }

  onPostDeleted(id: string) {
    this.posts.update(p => p.filter(x => x.id !== id));
  }
}
