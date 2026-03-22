import { Injectable, signal, computed } from '@angular/core';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { ApiService } from './api.service';
import { AuthResponse, AuthUser } from '../models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly TOKEN_KEY = 'ng_access';
  private readonly REFRESH_KEY = 'ng_refresh';

  private _user = signal<AuthUser | null>(this.loadUser());
  readonly user = this._user.asReadonly();
  readonly isLoggedIn = computed(() => this._user() !== null);

  constructor(private api: ApiService, private router: Router) {}

  register(username: string, email: string, password: string, displayName?: string) {
    return this.api.post<AuthResponse>('auth/register', { username, email, password, displayName })
      .pipe(tap(r => this.persist(r)));
  }

  login(email: string, password: string) {
    return this.api.post<AuthResponse>('auth/login', { email, password })
      .pipe(tap(r => this.persist(r)));
  }

  logout() {
    const refreshToken = localStorage.getItem(this.REFRESH_KEY);
    if (refreshToken) this.api.post('auth/logout', { refreshToken }).subscribe();
    this.clear();
    this.router.navigate(['/login']);
  }

  refresh() {
    const refreshToken = localStorage.getItem(this.REFRESH_KEY);
    if (!refreshToken) return;
    return this.api.post<AuthResponse>('auth/refresh', { refreshToken })
      .pipe(tap(r => this.persist(r)));
  }

  getToken() { return localStorage.getItem(this.TOKEN_KEY); }

  private persist(r: AuthResponse) {
    localStorage.setItem(this.TOKEN_KEY, r.accessToken);
    localStorage.setItem(this.REFRESH_KEY, r.refreshToken);
    localStorage.setItem('ng_user', JSON.stringify(r.user));
    this._user.set(r.user);
  }

  private clear() {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_KEY);
    localStorage.removeItem('ng_user');
    this._user.set(null);
  }

  private loadUser(): AuthUser | null {
    try { return JSON.parse(localStorage.getItem('ng_user') ?? 'null'); }
    catch { return null; }
  }
}
