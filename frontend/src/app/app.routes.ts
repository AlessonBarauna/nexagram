import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./layout/shell/shell.component').then(m => m.ShellComponent),
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'feed', pathMatch: 'full' },
      { path: 'feed', loadComponent: () => import('./features/feed/feed.component').then(m => m.FeedComponent) },
      { path: 'explore', loadComponent: () => import('./features/explore/explore.component').then(m => m.ExploreComponent) },
      { path: 'profile/:username', loadComponent: () => import('./features/profile/profile.component').then(m => m.ProfileComponent) },
      { path: 'post/:id', loadComponent: () => import('./features/post-detail/post-detail.component').then(m => m.PostDetailComponent) },
      { path: 'notifications', loadComponent: () => import('./features/notifications/notifications.component').then(m => m.NotificationsComponent) },
      { path: 'messages', loadComponent: () => import('./features/messages/messages.component').then(m => m.MessagesComponent) },
      { path: 'messages/:username', loadComponent: () => import('./features/messages/messages.component').then(m => m.MessagesComponent) },
      { path: 'search', loadComponent: () => import('./features/search/search.component').then(m => m.SearchComponent) },
    ]
  },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent),
    canActivate: [guestGuard]
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent),
    canActivate: [guestGuard]
  },
  { path: '**', redirectTo: 'feed' }
];
