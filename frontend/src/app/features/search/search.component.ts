import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { SearchService } from '../../core/services/search.service';
import { Post } from '../../core/models';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { AvatarModule } from 'primeng/avatar';
import { SkeletonModule } from 'primeng/skeleton';
import { debounceTime, distinctUntilChanged, Subject, switchMap } from 'rxjs';

interface SearchUser {
  id: string;
  username: string;
  displayName: string;
  avatarUrl: string | null;
  followersCount: number;
}

@Component({
  selector: 'app-search',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, InputTextModule, ButtonModule, AvatarModule, SkeletonModule],
  templateUrl: './search.component.html',
  styleUrl: './search.component.scss'
})
export class SearchComponent implements OnInit {
  private searchService = inject(SearchService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  query = '';
  users = signal<SearchUser[]>([]);
  posts = signal<Post[]>([]);
  hashtags = signal<{ name: string; postCount: number }[]>([]);
  loading = signal(false);
  searched = signal(false);

  private search$ = new Subject<string>();

  ngOnInit() {
    this.search$.pipe(
      debounceTime(400),
      distinctUntilChanged(),
      switchMap(q => {
        if (!q.trim()) return [];
        this.loading.set(true);
        return this.searchService.search(q);
      })
    ).subscribe({
      next: (r) => {
        this.users.set(r.users);
        this.posts.set(r.posts);
        this.hashtags.set(r.hashtags);
        this.loading.set(false);
        this.searched.set(true);
      },
      error: () => this.loading.set(false)
    });

    this.route.queryParams.subscribe(p => {
      if (p['q']) {
        this.query = p['q'];
        this.search$.next(this.query);
      }
    });
  }

  onInput() {
    this.search$.next(this.query);
  }
}
