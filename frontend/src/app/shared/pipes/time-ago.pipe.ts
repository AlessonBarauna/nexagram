import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'timeAgo', standalone: true, pure: false })
export class TimeAgoPipe implements PipeTransform {
  transform(value: string | Date): string {
    const now = Date.now();
    const then = new Date(value).getTime();
    const diff = Math.floor((now - then) / 1000);

    if (diff < 60)         return 'agora';
    if (diff < 3600)       return `${Math.floor(diff / 60)}m`;
    if (diff < 86400)      return `${Math.floor(diff / 3600)}h`;
    if (diff < 604800)     return `${Math.floor(diff / 86400)}d`;
    if (diff < 2592000)    return `${Math.floor(diff / 604800)}sem`;
    return new Date(value).toLocaleDateString('pt-BR', { day: 'numeric', month: 'short' });
  }
}
