import { Component, inject, signal } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-customer-shell',
  imports: [RouterLink, RouterOutlet],
  template: `
    <header>
      <a class="brand" routerLink="/">
        <b>EP</b>
        <strong>EventPark</strong>
      </a>

      <button class="menu" (click)="open.set(!open())">☰</button>

      <nav [class.open]="open()">
        <a routerLink="/">Home</a>
        <a routerLink="/events">Browse Events</a>

        @if (auth.isLoggedIn()) {
          <a routerLink="/dashboard">Dashboard</a>
          <a routerLink="/my-bookings">My Bookings</a>
          <a routerLink="/notifications">🔔</a>
          <a routerLink="/profile" class="account">
            {{ auth.user()?.fullName || 'Account' }}
          </a>
          <button (click)="auth.logout()">Logout</button>
        } @else {
          <a routerLink="/login">Login</a>
          <a routerLink="/register" class="account">Register</a>
        }
      </nav>
    </header>

    <router-outlet />

    <footer>
      <div class="page foot">
        <div>
          <div class="brand">
            <b>EP</b>
            <strong>EventPark</strong>
          </div>
          <p>Seats and parking, reserved together in one flow.</p>
        </div>

        <div>
          <strong>Discover</strong>
          <a routerLink="/events">Browse events</a>
          <a routerLink="/dashboard">Dashboard</a>
          <a routerLink="/my-bookings">My bookings</a>
        </div>

        <div>
          <strong>Account</strong>
          <a routerLink="/profile">Profile</a>
          <a routerLink="/notifications">Notifications</a>
        </div>
      </div>
    </footer>
  `,
  styles: [`
    header {
      height: 68px;
      background: #fff;
      border-bottom: 1px solid var(--border);
      display: flex;
      align-items: center;
      padding: 0 max(18px, calc((100% - 1180px) / 2));
      gap: 25px;
      position: sticky;
      top: 0;
      z-index: 20;
    }

    .brand { display: flex; align-items: center; gap: 8px; }

    .brand b {
      width: 30px;
      height: 30px;
      border-radius: 10px;
      background: var(--gradient);
      color: #fff;
      display: grid;
      place-items: center;
      font-size: 10px;
    }

    nav {
      margin-left: auto;
      display: flex;
      align-items: center;
      gap: 20px;
      font-size: 13px;
    }

    nav button { border: 0; background: transparent; }

    .account {
      padding: 9px 12px;
      border: 1px solid var(--border);
      border-radius: 11px;
    }

    .menu {
      display: none;
      margin-left: auto;
      border: 0;
      background: #fff;
      font-size: 21px;
    }

    footer {
      background: #fff;
      border-top: 1px solid var(--border);
      margin-top: 70px;
      padding: 42px 0;
    }

    .foot {
      display: grid;
      grid-template-columns: 2fr 1fr 1fr;
      gap: 35px;
      color: var(--muted);
      font-size: 12px;
    }

    .foot > div { display: flex; flex-direction: column; gap: 9px; }
    .foot strong { color: var(--text); }

    @media (max-width: 760px) {
      .menu { display: block; }

      nav {
        display: none;
        position: absolute;
        left: 10px;
        right: 10px;
        top: 62px;
        background: #fff;
        padding: 14px;
        border: 1px solid var(--border);
        border-radius: 14px;
        box-shadow: 0 10px 30px #0002;
        flex-direction: column;
        align-items: stretch;
      }

      nav.open { display: flex; }
      .foot { grid-template-columns: 1fr; }
    }
  `]
})
export class CustomerShellComponent {
  readonly auth = inject(AuthService);
  readonly open = signal(false);
}
