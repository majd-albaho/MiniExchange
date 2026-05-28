import { Component, computed, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from '../navbar/navbar.component';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { NotificationsToastComponent } from '../notifications-toast/notifications-toast.component';
import { ThemeService } from '../../../core/services/theme.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, NavbarComponent, SidebarComponent, NotificationsToastComponent],
  template: `
    <div class="app-shell" [attr.data-theme]="themeService.theme()">
      <app-sidebar />
      <div class="main-content">
        <app-navbar />
        <main class="page-content">
          <router-outlet />
        </main>
      </div>
      <app-notifications-toast />
    </div>
  `,
  styles: [`
    .app-shell {
      display: flex;
      height: 100vh;
      overflow: hidden;
      background: var(--bg-primary);
      color: var(--text-primary);
    }
    .main-content {
      flex: 1;
      display: flex;
      flex-direction: column;
      overflow: hidden;
    }
    .page-content {
      flex: 1;
      overflow-y: auto;
      padding: 24px;
    }
  `],
})
export class MainLayoutComponent {
  themeService = inject(ThemeService);
}
