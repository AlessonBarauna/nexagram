// ── Auth ───────────────────────────────────────────────────────────────────
export interface AuthUser {
  id: string;
  username: string;
  displayName?: string;
  avatarUrl?: string;
  isVerified: boolean;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: AuthUser;
}

// ── User ───────────────────────────────────────────────────────────────────
export interface UserProfile {
  id: string;
  username: string;
  displayName?: string;
  bio?: string;
  avatarUrl?: string;
  website?: string;
  isVerified: boolean;
  isPrivate: boolean;
  followerCount: number;
  followingCount: number;
  postCount: number;
  createdAt: string;
}

export interface UserSummary {
  id: string;
  username: string;
  displayName?: string;
  avatarUrl?: string;
  isVerified: boolean;
}

// ── Posts ──────────────────────────────────────────────────────────────────
export interface MediaItem {
  url: string;
  mimeType: string;
}

export type PostVisibility = 'Public' | 'Followers' | 'CloseFriends' | 'Private';
export type PostStatus = 'Published' | 'Draft' | 'Scheduled' | 'Deleted';

export interface Post {
  id: string;
  author: UserSummary;
  caption?: string;
  media: MediaItem[];
  location?: string;
  visibility: PostVisibility;
  status: PostStatus;
  likeCount: number;
  commentCount: number;
  saveCount: number;
  viewCount: number;
  aiTags?: string[];
  isLikedByMe?: boolean;
  isSavedByMe?: boolean;
  createdAt: string;
  updatedAt: string;
}

// ── Comments ───────────────────────────────────────────────────────────────
export interface Comment {
  id: string;
  author: UserSummary;
  content: string;
  likeCount: number;
  isPinned: boolean;
  parentId?: string;
  replies: Comment[];
  createdAt: string;
}

// ── Stories ────────────────────────────────────────────────────────────────
export interface Story {
  id: string;
  author: UserSummary;
  mediaUrl: string;
  mediaType: string;
  durationSeconds: number;
  expiresAt: string;
  viewCount: number;
  hasViewed: boolean;
  createdAt: string;
}

export interface StoryFeedItem {
  user: UserSummary;
  stories: Story[];
  hasUnviewed: boolean;
}

// ── Notifications ──────────────────────────────────────────────────────────
export type NotificationType = 'Follow' | 'Like' | 'Comment' | 'Mention' | 'StoryView' | 'CollabInvite';

export interface Notification {
  id: string;
  type: NotificationType;
  actor: UserSummary;
  entityId?: string;
  entityType?: string;
  isRead: boolean;
  createdAt: string;
}

// ── Direct Messages ────────────────────────────────────────────────────────
export interface DirectMessage {
  id: string;
  sender: UserSummary;
  receiver: UserSummary;
  content?: string;
  mediaUrl?: string;
  sharedPost?: Post;
  isRead: boolean;
  isEphemeral: boolean;
  expiresAt?: string;
  createdAt: string;
}

export interface Conversation {
  participant: UserSummary;
  lastMessage: DirectMessage;
  unreadCount: number;
}

// ── Shared ─────────────────────────────────────────────────────────────────
export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNext: boolean;
  hasPrev: boolean;
}
