import { Component, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector:'app-customer-shell',
  imports:[RouterLink,RouterLinkActive,RouterOutlet],
  template:`
    <div class="customer-app">
      <header class="topbar">
        <a class="brand" routerLink="/" aria-label="EventPark home">
          <img src="/customer-assets/eventpark-logo.svg" alt="EventPark">
        </a>

        <button class="menu" type="button" aria-label="Open navigation" (click)="open.set(!open())">☰</button>

        <nav [class.open]="open()">
          <a routerLink="/" routerLinkActive="active" [routerLinkActiveOptions]="{exact:true}">Home</a>
          <a routerLink="/events" routerLinkActive="active">Explore Events</a>

          @if(auth.isCustomer()){
            <a routerLink="/dashboard" routerLinkActive="active">Dashboard</a>
            <a routerLink="/my-bookings" routerLinkActive="active">My Bookings</a>
            <a routerLink="/notifications" class="icon-link" aria-label="Notifications">◉</a>
            <a routerLink="/profile" class="account">{{auth.user()?.fullName || 'Account'}}</a>
            <button class="logout" type="button" (click)="auth.logout()">Logout</button>
          } @else {
            <a routerLink="/login">Login</a>
            <a routerLink="/register" class="account">Create account</a>
          }
        </nav>
      </header>

      <router-outlet />

      <footer>
        <div class="page footer-grid">
          <div class="about">
            <a class="footer-brand" routerLink="/" aria-label="EventPark home">
              <span class="footer-mark">P</span><strong>EventPark</strong>
            </a>
            <p>Reserve event seats and optional parking in one simple, protected booking flow.</p>
          </div>
          <div><strong>Discover</strong><a routerLink="/events">Browse events</a><a routerLink="/dashboard">Dashboard</a><a routerLink="/my-bookings">My bookings</a></div>
          <div><strong>Account</strong><a routerLink="/profile">Profile</a><a routerLink="/notifications">Notifications</a><a routerLink="/login">Login</a></div>
          <div><strong>Experience</strong><span>Live seat availability</span><span>5-minute protected hold</span><span>Optional parking</span></div>
        </div>
      </footer>
    </div>
  `,
  styles:[`
    :host{
      --navy:#07363a;
      --accent:#0b7a69;
      --blue:#12a68c;
      --bg:#f4f7f2;
      --surface:#ffffff;
      --text:#132326;
      --muted:#6b7b7e;
      --border:#dce7e4;
      --danger:#c2410c;
      --success:#15803d;
      --gradient:linear-gradient(135deg,#0b1f2a 0%,#0b7a69 68%,#f3b21a 140%);
      display:block;
      min-height:100vh;
      background:var(--bg);
      color:var(--text);
    }

    .customer-app{min-height:100vh;background:var(--bg)}
    .topbar{
      min-height:72px;background:rgba(255,255,255,.96);border-bottom:1px solid var(--border);
      display:flex;align-items:center;padding:0 max(20px,calc((100% - 1180px)/2));
      gap:28px;position:sticky;top:0;z-index:50;backdrop-filter:blur(14px)
    }
    .brand img{width:154px;height:auto;display:block}
    nav{margin-left:auto;display:flex;align-items:center;gap:22px;font-size:13px}
    nav a{color:#42575a;transition:.2s}nav a:hover,nav a.active{color:var(--accent);font-weight:800}
    .account{padding:10px 14px;border:1px solid var(--border);border-radius:12px;background:#fff}
    .icon-link{width:34px;height:34px;display:grid;place-items:center;border-radius:50%;background:#eaf7f4}
    .logout{border:0;background:transparent;color:#647477;cursor:pointer}
    .menu{display:none;margin-left:auto;border:0;background:#fff;font-size:22px}
    footer{margin-top:84px;padding:46px 0;background:#07363a;color:#d4e2df}
    .footer-grid{display:grid;grid-template-columns:2fr repeat(3,1fr);gap:38px;font-size:12px}
    .footer-grid>div{display:flex;flex-direction:column;gap:10px}.footer-grid strong{color:#fff}
    .footer-brand{display:inline-flex;align-items:center;gap:10px;width:max-content}.footer-mark{width:34px;height:34px;border-radius:10px;background:linear-gradient(135deg,#0b7a69,#f3b21a);color:#fff;display:grid;place-items:center;font-weight:950;font-size:16px;box-shadow:inset 0 0 0 5px #07363a}.footer-brand strong{font-size:18px;letter-spacing:-.3px;color:#fff}.about p{max-width:340px;line-height:1.7}
    @media(max-width:820px){
      .menu{display:block}
      nav{display:none;position:absolute;left:12px;right:12px;top:64px;background:#fff;border:1px solid var(--border);border-radius:16px;padding:14px;box-shadow:0 20px 50px #07363a22;flex-direction:column;align-items:stretch}
      nav.open{display:flex}.footer-grid{grid-template-columns:1fr 1fr}
    }
    @media(max-width:520px){.footer-grid{grid-template-columns:1fr}.topbar{padding:0 14px}.brand img{width:138px}}
  `]
})
export class CustomerShellComponent{
  readonly auth=inject(AuthService);
  readonly open=signal(false);
}