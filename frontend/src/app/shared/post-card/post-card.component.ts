import { Component, EventEmitter, inject, Input, OnInit, Output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Post, Comment } from '../../core/models';
import { PostService } from '../../core/services/post.service';
import { AuthService } from '../../core/services/auth.service';
import { AvatarModule } from 'primeng/avatar';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { MenuModule } from 'primeng/menu';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService, MenuItem, MessageService } from 'primeng/api';
import { TooltipModule } from 'primeng/tooltip';
import { TimeAgoPipe } from '../pipes/time-ago.pipe';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

@Component({
  selector: 'app-post-card',
  standalone: true,
  imports: [
    CommonModule, RouterLink, FormsModule,
    AvatarModule, ButtonModule, InputTextModule, MenuModule,
    ConfirmDialogModule, TooltipModule, TimeAgoPipe
  ],
  templateUrl: './post-card.component.html',
  styleUrl: './post-card.component.scss'
})
export class PostCardComponent implements OnInit {
  @Input({ required: true }) post!: Post;
  @Output() deleted = new EventEmitter<string>();

  private postService = inject(PostService);
  private auth = inject(AuthService);
  private confirmService = inject(ConfirmationService);
  private messageService = inject(MessageService);
  private sanitizer = inject(DomSanitizer);

  comments = signal<Comment[]>([]);
  showComments = signal(false);
  commentText = '';
  mediaIndex = signal(0);
  liked = signal(false);
  saved = signal(false);
  likesCount = signal(0);
  menuItems: MenuItem[] = [];

  get isOwner() { return this.auth.user()?.id === this.post.author.id; }

  isImage(mimeType: string) { return mimeType.startsWith('image/'); }

  formatCaption(text: string | undefined): SafeHtml {
    if (!text) return '';
    const html = text.replace(/#(\w+)/g, '<a class="caption-tag" href="/search?q=$1">#$1</a>');
    return this.sanitizer.bypassSecurityTrustHtml(html);
  }

  ngOnInit() {
    this.liked.set(this.post.isLikedByMe ?? false);
    this.saved.set(this.post.isSavedByMe ?? false);
    this.likesCount.set(this.post.likeCount ?? 0);
    this.menuItems = this.isOwner
      ? [{ label: 'Deletar', icon: 'pi pi-trash', command: () => this.confirmDelete() }]
      : [{ label: 'Reportar', icon: 'pi pi-flag' }];
  }

  toggleLike() {
    if (this.liked()) {
      this.liked.set(false);
      this.likesCount.update(n => n - 1);
      this.postService.unlike(this.post.id).subscribe();
    } else {
      this.liked.set(true);
      this.likesCount.update(n => n + 1);
      this.postService.like(this.post.id).subscribe();
    }
  }

  toggleSave() {
    this.saved.update(v => !v);
    (this.saved() ? this.postService.save(this.post.id) : this.postService.unsave(this.post.id)).subscribe();
  }

  toggleComments() {
    this.showComments.update(v => !v);
    if (this.showComments() && !this.comments().length) this.loadComments();
  }

  loadComments() {
    this.postService.getComments(this.post.id).subscribe(r => this.comments.set(r.items));
  }

  submitComment() {
    if (!this.commentText.trim()) return;
    this.postService.addComment(this.post.id, this.commentText).subscribe(c => {
      this.comments.update(list => [c, ...list]);
      this.commentText = '';
    });
  }

  prevMedia() { this.mediaIndex.update(i => Math.max(0, i - 1)); }
  nextMedia() { this.mediaIndex.update(i => Math.min((this.post.media?.length ?? 1) - 1, i + 1)); }

  confirmDelete() {
    this.confirmService.confirm({
      message: 'Tem certeza que deseja deletar este post?',
      header: 'Deletar post',
      icon: 'pi pi-trash',
      acceptLabel: 'Deletar',
      rejectLabel: 'Cancelar',
      accept: () => this.postService.delete(this.post.id).subscribe(() => this.deleted.emit(this.post.id))
    });
  }
}
