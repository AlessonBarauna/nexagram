import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, InputTextModule, PasswordModule, ButtonModule, MessageModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent {
  private auth = inject(AuthService);
  private router = inject(Router);

  username = '';
  email = '';
  password = '';
  displayName = '';
  loading = signal(false);
  error = signal('');

  submit() {
    if (!this.username || !this.email || !this.password) return;
    this.loading.set(true);
    this.error.set('');
    this.auth.register(this.username, this.email, this.password, this.displayName || undefined).subscribe({
      next: () => this.router.navigate(['/']),
      error: (e) => {
        this.error.set(e.error?.message || 'Erro ao criar conta');
        this.loading.set(false);
      }
    });
  }
}
