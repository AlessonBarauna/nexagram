import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FeedService } from '../../core/services/feed.service';
import { SearchService } from '../../core/services/search.service';
import { Post } from '../../core/models';
import { SkeletonModule } from 'primeng/skeleton';
import { ChipModule } from 'primeng/chip';

@Component({
  selector: 'app-explore',
  standalone: true,
  imports: [CommonModule, RouterLink, SkeletonModule, ChipModule],
  templateUrl: './explore.component.html',
  styleUrl: './explore.component.scss'
})
export class ExploreComponent implements OnInit {
  private feedService = inject(FeedService);
  private searchService = inject(SearchService);

  posts = signal<Post[]>([]);
  trending = signal<{ name: string; postCount: number }[]>([]);
  loading = signal(true);

  ngOnInit() {
    this.feedService.getExplore().subscribe({
      next: (r) => { this.posts.set(r.items); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
    this.searchService.getTrending().subscribe(t => this.trending.set(t.slice(0, 10)));
  }
}
