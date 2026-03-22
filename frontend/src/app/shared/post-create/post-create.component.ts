import { Component, EventEmitter, inject, Output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PostService } from '../../core/services/post.service';
import { StorageService } from '../../core/services/storage.service';
import { Post } from '../../core/models';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-post-create',
  standalone: true,
  imports: [CommonModule, FormsModule, DialogModule, ButtonModule, InputTextareaModule],
  templateUrl: './post-create.component.html',
  styleUrl: './post-create.component.scss'
})
export class PostCreateComponent {
  @Output() created = new EventEmitter<Post>();

  private postService = inject(PostService);
  private storageService = inject(StorageService);
  private messageService = inject(MessageService);

  visible = signal(false);
  caption = '';
  visibility = 'Public';
  selectedFiles: File[] = [];
  previews = signal<{ url: string; type: 'image' | 'video' }[]>([]);
  loading = signal(false);
  uploadProgress = signal('');
  step = signal<'upload' | 'caption'>('upload');
  dragOver = signal(false);

  visibilityOptions = [
    { label: 'Público', value: 'Public' },
    { label: 'Seguidores', value: 'Followers' },
    { label: 'Privado', value: 'Private' },
  ];

  open() { this.visible.set(true); this.reset(); }
  close() { this.visible.set(false); this.reset(); }

  reset() {
    this.caption = '';
    this.visibility = 'Public';
    this.selectedFiles = [];
    this.previews.set([]);
    this.step.set('upload');
    this.loading.set(false);
    this.uploadProgress.set('');
    this.dragOver.set(false);
  }

  setFiles(files: File[]) {
    if (!files.length) return;
    this.selectedFiles = files;
    const prevs = files.map(f => ({
      url: URL.createObjectURL(f),
      type: (f.type.startsWith('video') ? 'video' : 'image') as 'image' | 'video'
    }));
    this.previews.set(prevs);
    this.step.set('caption');
  }

  onFileSelect(event: Event) {
    const input = event.target as HTMLInputElement;
    this.setFiles(Array.from(input.files ?? []));
  }

  onDrop(event: DragEvent) {
    event.preventDefault();
    this.dragOver.set(false);
    this.setFiles(Array.from(event.dataTransfer?.files ?? []));
  }

  onDragOver(event: DragEvent) { event.preventDefault(); this.dragOver.set(true); }
  onDragLeave() { this.dragOver.set(false); }

  submit() {
    if (!this.selectedFiles.length) return;
    this.loading.set(true);
    this.uploadProgress.set('Enviando arquivos...');

    this.storageService.uploadFiles(this.selectedFiles).subscribe({
      next: (media) => {
        this.uploadProgress.set('Publicando...');
        this.postService.create({ caption: this.caption, visibility: this.visibility, media }).subscribe({
          next: (post) => {
            this.created.emit(post);
            this.close();
            this.messageService.add({ severity: 'success', summary: 'Post publicado!' });
          },
          error: (e) => {
            this.messageService.add({ severity: 'error', summary: 'Erro ao publicar', detail: e.error?.message });
            this.loading.set(false);
            this.uploadProgress.set('');
          }
        });
      },
      error: (e) => {
        this.messageService.add({ severity: 'error', summary: 'Erro no upload', detail: e.message });
        this.loading.set(false);
        this.uploadProgress.set('');
      }
    });
  }
}
