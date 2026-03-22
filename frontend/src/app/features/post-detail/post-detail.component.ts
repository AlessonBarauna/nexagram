import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { PostService } from '../../core/services/post.service';
import { Post } from '../../core/models';
import { PostCardComponent } from '../../shared/post-card/post-card.component';
import { SkeletonModule } from 'primeng/skeleton';

@Component({
  selector: 'app-post-detail',
  standalone: true,
  imports: [CommonModule, PostCardComponent, SkeletonModule],
  template: `
    <div style="max-width:600px; margin:2rem auto; padding:0 1rem;">
      @if (loading()) {
        <p-skeleton width="100%" height="400px" borderRadius="12px" />
      } @else if (post()) {
        <app-post-card [post]="post()!" (deleted)="onDeleted()" />
      } @else {
        <div style="text-align:center;padding:4rem;color:var(--text-color-secondary)">
          <i class="pi pi-exclamation-triangle" style="font-size:3rem"></i>
          <p>Post não encontrado</p>
        </div>
      }
    </div>
  `
})
export class PostDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private postService = inject(PostService);

  post = signal<Post | null>(null);
  loading = signal(true);

  ngOnInit() {
    const id = this.route.snapshot.params['id'];
    this.postService.getById(id).subscribe({
      next: (p) => { this.post.set(p); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  onDeleted() {
    this.router.navigate(['/feed']);
  }
}
