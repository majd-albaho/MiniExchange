import { Injectable, signal } from '@angular/core';

export type ThemeMode = 'dark' | 'light';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly THEME_KEY = 'mx_theme';
  readonly theme = signal<ThemeMode>(this.loadTheme());

  private loadTheme(): ThemeMode {
    return (localStorage.getItem(this.THEME_KEY) as ThemeMode) ?? 'dark';
  }

  toggleTheme(): void {
    const next = this.theme() === 'dark' ? 'light' : 'dark';
    this.theme.set(next);
    localStorage.setItem(this.THEME_KEY, next);
    document.body.setAttribute('data-theme', next);
  }

  applyTheme(): void {
    document.body.setAttribute('data-theme', this.theme());
  }
}
