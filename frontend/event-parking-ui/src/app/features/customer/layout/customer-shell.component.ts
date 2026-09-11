import { Component, inject, signal, ViewEncapsulation } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector:'app-customer-shell',
  imports:[RouterLink,RouterLinkActive,RouterOutlet],
  encapsulation:ViewEncapsulation.None,
  template:`
    <div class="customer-app">
      <header class="customer-topbar">
        <a class="brand" routerLink="/" aria-label="EventPark home">
          <img src="/customer-assets/eventpark-logo.svg" alt="EventPark">
        </a>

        <button class="menu" type="button" aria-label="Open navigation" (click)="open.set(!open())">☰</button>

        <nav class="main-nav" [class.open]="open()">
          <a routerLink="/" routerLinkActive="active" [routerLinkActiveOptions]="{exact:true}" (click)="open.set(false)">Home</a>
          <a routerLink="/events" routerLinkActive="active" (click)="open.set(false)">Explore Events</a>

          @if(auth.isCustomer()){
            <a routerLink="/dashboard" routerLinkActive="active" (click)="open.set(false)">Dashboard</a>
            <a routerLink="/my-bookings" routerLinkActive="active" (click)="open.set(false)">My Bookings</a>
            <a routerLink="/notifications" routerLinkActive="active" class="notification-link" (click)="open.set(false)">
              <span>●</span> Notifications
            </a>
            <a routerLink="/profile" routerLinkActive="active" class="account" (click)="open.set(false)">
              <i>{{initial()}}</i>
              <span>{{auth.user()?.fullName || 'Account'}}</span>
            </a>
            <button class="logout" type="button" (click)="auth.logout()">Logout</button>
          } @else {
            <a routerLink="/login" (click)="open.set(false)">Login</a>
            <a routerLink="/register" class="create-account" (click)="open.set(false)">Create account</a>
          }
        </nav>
      </header>

      <div class="customer-quickbar">
        <div class="quick-inner">
          <span class="quick-label">QUICK ACCESS</span>
          <a routerLink="/events">Find events</a>
          @if(auth.isCustomer()){
            <a routerLink="/my-bookings">My tickets</a>
            <a routerLink="/notifications">Updates</a>
            <a routerLink="/profile">Profile</a>
          } @else {
            <a routerLink="/register">Join EventPark</a>
          }
        </div>
      </div>

      <main class="customer-content">
        <router-outlet />
      </main>

      <footer class="customer-footer">
        <div class="page footer-grid">
          <div class="footer-brand-block">
            <a class="footer-brand" routerLink="/">
              <span class="footer-mark">EP</span>
              <div><strong>EventPark</strong><small>Park smart • Event better</small></div>
            </a>
            <p>Discover events, reserve exact seats, add parking and keep every booking in one connected experience.</p>
          </div>

          <div><strong>Discover</strong><a routerLink="/events">Browse events</a><a routerLink="/dashboard">Dashboard</a><a routerLink="/my-bookings">My bookings</a></div>
          <div><strong>Account</strong><a routerLink="/profile">Profile</a><a routerLink="/notifications">Notifications</a><a routerLink="/login">Sign in</a></div>
          <div><strong>Booking flow</strong><span>Live seat status</span><span>Protected hold</span><span>Optional parking</span><span>QR ticket</span></div>
        </div>
        <div class="page footer-bottom"><span>EventPark Customer Experience</span><span>Seat • Park • Pay • Enjoy</span></div>
      </footer>
    </div>
  `,
  styles:[`
    .customer-app{
      --c-navy:#07383a;
      --c-navy2:#052e32;
      --c-teal:#0e8f79;
      --c-teal2:#20b89b;
      --c-mint:#eaf8f4;
      --c-gold:#e5aa22;
      --c-ink:#17343a;
      --c-muted:#6d8388;
      --c-border:#9fd4ca;
      --c-surface:rgba(255,255,255,.90);
      min-height:100vh;
      color:var(--c-ink);
      background:
        linear-gradient(rgba(250,255,254,.48),rgba(250,255,254,.48)),
        url('/customer-assets/customer-operations-bg.png') center top / cover fixed no-repeat;
      font-family:Inter,ui-sans-serif,system-ui,-apple-system,BlinkMacSystemFont,"Segoe UI",sans-serif;
    }

    .customer-topbar{
      min-height:74px;
      display:flex;align-items:center;gap:24px;
      padding:0 max(18px,calc((100% - 1220px)/2));
      position:sticky;top:0;z-index:70;
      background:rgba(255,255,255,.88);
      backdrop-filter:blur(18px);
      border-bottom:1px solid rgba(121,189,177,.48);
      box-shadow:0 6px 22px rgba(6,64,64,.055);
    }
    .brand img{width:154px;display:block}
    .main-nav{margin-left:auto;display:flex;align-items:center;gap:18px;font-size:13px}
    .main-nav>a{color:#3d585d;text-decoration:none;font-weight:700;padding:10px 2px;position:relative}
    .main-nav>a:not(.account):after{content:"";position:absolute;left:0;right:0;bottom:3px;height:2px;border-radius:99px;background:var(--c-teal);transform:scaleX(0);transition:.2s}
    .main-nav>a:hover:after,.main-nav>a.active:after{transform:scaleX(1)}
    .main-nav>a:hover,.main-nav>a.active{color:var(--c-teal)}
    .notification-link span{font-size:8px;color:#e09b13;margin-right:3px}
    .account{
      display:flex!important;align-items:center;gap:8px!important;
      padding:6px 10px 6px 7px!important;
      border:1px solid #b7ddd6;border-radius:12px;background:#fff;
    }
    .account i{width:30px;height:30px;border-radius:9px;display:grid;place-items:center;background:linear-gradient(135deg,var(--c-navy),var(--c-teal));color:#fff;font-style:normal;font-size:10px;font-weight:950}
    .logout{border:0;background:transparent;color:#667d82;font-weight:800;cursor:pointer}
    .create-account{padding:10px 14px!important;border-radius:10px;background:linear-gradient(135deg,var(--c-navy),var(--c-teal));color:#fff!important}
    .create-account:after{display:none!important}
    .menu{display:none;margin-left:auto;border:1px solid #b7ddd6;background:#fff;color:var(--c-navy);width:40px;height:40px;border-radius:10px;font-size:18px}

    .customer-quickbar{
      position:sticky;top:74px;z-index:55;
      background:rgba(239,251,248,.91);backdrop-filter:blur(12px);
      border-bottom:1px solid rgba(121,189,177,.42);
    }
    .quick-inner{max-width:1220px;margin:auto;min-height:44px;display:flex;align-items:center;gap:7px;padding:0 18px;overflow:auto}
    .quick-label{font-size:8px;letter-spacing:1.4px;font-weight:950;color:#6e878b;margin-right:4px;white-space:nowrap}
    .quick-inner a{white-space:nowrap;padding:7px 11px;border:1px solid #b8ddd6;border-radius:9px;background:#ffffffd9;color:#35565b;text-decoration:none;font-size:10px;font-weight:850}
    .quick-inner a:hover{background:var(--c-teal);border-color:var(--c-teal);color:#fff}

    .customer-content{min-height:calc(100vh - 118px)}
    .customer-content .page{position:relative;z-index:1}
    .customer-content .card,.customer-content .panel,.customer-content .item,.customer-content .booking,.customer-content .note,.customer-content .form{
      border-color:rgba(111,190,176,.48)!important;
      box-shadow:0 16px 38px rgba(9,83,80,.065)!important;
    }

    .customer-footer{margin-top:76px;padding:44px 0 20px;background:linear-gradient(135deg,#06393b,#052c31);color:#d4e9e5}
    .footer-grid{display:grid;grid-template-columns:1.7fr repeat(3,1fr);gap:38px;font-size:12px}
    .footer-grid>div{display:flex;flex-direction:column;gap:9px}
    .footer-grid strong{color:#fff}
    .footer-grid a{color:#d2e5e1;text-decoration:none}.footer-grid a:hover{color:#fff}
    .footer-brand{display:flex;align-items:center;gap:10px;width:max-content}
    .footer-mark{width:40px;height:40px;border-radius:12px;background:linear-gradient(135deg,#18b093,#e5aa22);color:white;display:grid;place-items:center;font-weight:950}
    .footer-brand>div{display:flex;flex-direction:column}.footer-brand strong{font-size:18px}.footer-brand small{font-size:8px;text-transform:uppercase;letter-spacing:1px;color:#95bbb4}
    .footer-brand-block p{max-width:360px;line-height:1.7;color:#a9c6c1}
    .footer-bottom{margin-top:28px;padding-top:17px;border-top:1px solid #ffffff14;display:flex;justify-content:space-between;color:#83aaa3;font-size:9px}


    /* v3.1 readability upgrade */
    .customer-app .main-nav{font-size:14px!important}
    .customer-app .quick-label{font-size:10px!important}
    .customer-app .quick-inner a{font-size:12px!important}
    .customer-app .customer-content p{font-size:14px}
    .customer-app .customer-content label{font-size:12px}
    .customer-app .customer-content button{font-size:11px}
    .customer-app .customer-content small{font-size:10px}
    .customer-app .customer-content .kicker{font-size:10px!important}
    .customer-app .customer-content h2{font-weight:850}
    .customer-app .customer-content h3{font-weight:850}
    .customer-app .footer-grid{font-size:13px}

    @media(max-width:900px){
      .menu{display:grid;place-items:center}
      .main-nav{
        display:none;position:absolute;left:12px;right:12px;top:66px;
        background:#fff;border:1px solid #b7ddd6;border-radius:16px;padding:14px;
        box-shadow:0 22px 55px rgba(7,56,58,.18);flex-direction:column;align-items:stretch;
      }
      .main-nav.open{display:flex}
      .footer-grid{grid-template-columns:1fr 1fr}
    }
    @media(max-width:560px){
      .brand img{width:138px}.customer-topbar{padding:0 14px}
      .quick-label{display:none}.footer-grid{grid-template-columns:1fr}
      .footer-bottom{flex-direction:column;gap:6px}
    }
  `]
})
export class CustomerShellComponent{
  readonly auth=inject(AuthService);
  readonly open=signal(false);
  initial(){return String(this.auth.user()?.fullName||'C').trim().charAt(0).toUpperCase()}
}
