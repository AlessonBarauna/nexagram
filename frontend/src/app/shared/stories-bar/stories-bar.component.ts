import { Component, Input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StoryFeedItem } from '../../core/models';
import { AvatarModule } from 'primeng/avatar';
import { DialogModule } from 'primeng/dialog';
import { StoryService } from '../../core/services/story.service';

@Component({
  selector: 'app-stories-bar',
  standalone: true,
  imports: [CommonModule, AvatarModule, DialogModule],
  templateUrl: './stories-bar.component.html',
  styleUrl: './stories-bar.component.scss'
})
export class StoriesBarComponent {
  @Input() stories: StoryFeedItem[] = [];

  viewingStory = signal<{ url: string; username: string; type: string } | null>(null);
  storyIndex = signal(0);
  currentGroup = signal<StoryFeedItem | null>(null);

  constructor(private storyService: StoryService) {}

  openStories(group: StoryFeedItem, idx = 0) {
    if (!group.stories.length) return;
    this.currentGroup.set(group);
    this.storyIndex.set(idx);
    const s = group.stories[idx];
    this.viewingStory.set({ url: s.mediaUrl, username: group.user.username, type: s.mediaType });
    this.storyService.view(s.id).subscribe();
  }

  next() {
    const g = this.currentGroup();
    if (!g) return;
    const next = this.storyIndex() + 1;
    if (next < g.stories.length) {
      this.openStories(g, next);
    } else {
      this.closeStory();
    }
  }

  closeStory() {
    this.viewingStory.set(null);
    this.currentGroup.set(null);
  }
}
