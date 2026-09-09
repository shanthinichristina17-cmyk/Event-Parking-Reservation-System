import { Component, inject, signal, ViewEncapsulation } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector:'app-admin-shell',
  imports:[RouterLink,RouterLinkActive,RouterOutlet],
  encapsulation:ViewEncapsulation.None,
  template:`
  <div class="admin-layout">
    <aside class="admin-sidebar" [class.open]="open()">
      <div class="admin-brand">
        <div class="brand-mark">EP</div>
        <div><strong>EventPark</strong><span>Operations Console</span></div>
        <button class="sidebar-close" (click)="open.set(false)">×</button>
      </div>

      <div class="nav-label">OVERVIEW</div>
      <nav class="admin-nav">
        <a routerLink="dashboard" routerLinkActive="on" (click)="open.set(false)"><span class="nav-icon">⌂</span><b>Dashboard</b></a>
      </nav>

      <div class="nav-label">MANAGEMENT</div>
      <nav class="admin-nav">
        <a routerLink="customers" routerLinkActive="on" (click)="open.set(false)"><span class="nav-icon">◎</span><b>Customers</b></a>
        <a routerLink="venues" routerLinkActive="on" (click)="open.set(false)"><span class="nav-icon">◆</span><b>Venues</b></a>
        <a routerLink="categories" routerLinkActive="on" (click)="open.set(false)"><span class="nav-icon">▤</span><b>Categories</b></a>
        <a routerLink="events" routerLinkActive="on" (click)="open.set(false)"><span class="nav-icon">◫</span><b>Events</b></a>
      </nav>

      <div class="nav-label">OPERATIONS</div>
      <nav class="admin-nav">
        <a routerLink="layout" routerLinkActive="on" (click)="open.set(false)"><span class="nav-icon">▦</span><b>Seats & Parking</b></a>
        <a routerLink="bookings" routerLinkActive="on" (click)="open.set(false)"><span class="nav-icon">▣</span><b>Bookings</b></a>
        <a routerLink="payments" routerLinkActive="on" (click)="open.set(false)"><span class="nav-icon">▧</span><b>Payments</b></a>
        <a routerLink="reports" routerLinkActive="on" (click)="open.set(false)"><span class="nav-icon">⌁</span><b>Reports</b></a>
      </nav>

      <div class="nav-label">SYSTEM</div>
      <nav class="admin-nav">
        <a routerLink="notifications" routerLinkActive="on" (click)="open.set(false)"><span class="nav-icon">◉</span><b>Notifications</b></a>
        <a routerLink="settings" routerLinkActive="on" (click)="open.set(false)"><span class="nav-icon">⚙</span><b>Settings</b></a>
      </nav>

      <div class="sidebar-footer">
        <div class="server-state"><span></span> System online</div>
        <button class="logout" (click)="auth.logout()">↪ <b>Logout</b></button>
      </div>
    </aside>

    @if(open()){<div class="sidebar-backdrop" (click)="open.set(false)"></div>}

    <section class="admin-content">
      <header class="admin-topbar">
        <button class="hamb" (click)="open.set(!open())">☰</button>
        <div class="top-title"><span>EVENTPARK</span><b>Operations Console</b></div>
        <div class="top-spacer"></div>
        <div class="admin-user">
          <span class="live-dot"></span>
          <div><small>Signed in as</small><strong>{{auth.user()?.fullName||'System Admin'}}</strong></div>
          <i>{{initial()}}</i>
        </div>
      </header>

      <div class="quick-nav">
        <span class="quick-title">Quick navigate</span>
        <a routerLink="dashboard" routerLinkActive="active"><b>Dashboard</b></a>
        <a routerLink="events" routerLinkActive="active"><b>Events</b></a>
        <a routerLink="layout" routerLinkActive="active"><b>Seat Map</b></a>
        <a routerLink="bookings" routerLinkActive="active"><b>Bookings</b></a>
        <a routerLink="payments" routerLinkActive="active"><b>Payments</b></a>
        <a routerLink="reports" routerLinkActive="active"><b>Reports</b></a>
        <a routerLink="settings" routerLinkActive="active"><b>Settings</b></a>
      </div>

      <div class="admin-page"><router-outlet/></div>
    </section>
  </div>
  `,
  styles:[`
    .admin-layout{
      --navy:#07383a;--navy2:#052f32;--teal:#18a88e;--teal2:#27bea2;
      --ink:#14343a;--muted:#6e858a;--border:#8fd2c4;--surface:rgba(255,255,255,.90);
      min-height:100vh;display:grid;grid-template-columns:230px minmax(0,1fr);
      color:var(--ink);font-family:Inter,ui-sans-serif,system-ui,-apple-system,"Segoe UI",sans-serif;
      background:
        linear-gradient(rgba(250,255,254,.20),rgba(250,255,254,.20)),
        url('/admin-assets/admin-operations-bg.png') center/cover fixed no-repeat;
    }

    .admin-sidebar{
      height:100vh;position:sticky;top:0;z-index:60;box-sizing:border-box;
      padding:18px 13px;display:flex;flex-direction:column;overflow:auto;
      color:#dff4ef;background:linear-gradient(180deg,rgba(4,62,64,.98),rgba(2,47,50,.98));
      box-shadow:13px 0 35px rgba(3,56,58,.15);
    }
    .admin-brand{display:flex;align-items:center;gap:10px;padding:3px 5px 18px;border-bottom:1px solid #ffffff17;margin-bottom:13px}
    .brand-mark{width:38px;height:38px;border-radius:11px;display:grid;place-items:center;background:linear-gradient(135deg,#28c4a5,#e9c64b);color:white;font-size:10px;font-weight:950}
    .admin-brand>div:nth-child(2){display:flex;flex-direction:column}
    .admin-brand strong{font-size:14px;color:white}.admin-brand span{font-size:7px;letter-spacing:1.2px;color:#9ccbc3;text-transform:uppercase}
    .sidebar-close{display:none;margin-left:auto;border:0;background:transparent;color:white;font-size:24px}
    .nav-label{font-size:7px;font-weight:950;letter-spacing:1.5px;color:#78aaa4;margin:11px 9px 6px}
    .admin-nav{display:flex;flex-direction:column;gap:4px}
    .admin-nav a{display:flex;align-items:center;gap:9px;padding:9px 9px;border-radius:9px;color:#d4ebe6;text-decoration:none;font-size:10px;border:1px solid transparent;transition:.16s}
    .admin-nav a b{font-weight:800}
    .nav-icon{width:20px;height:20px;border-radius:6px;display:grid;place-items:center;background:#ffffff12}
    .admin-nav a:hover{background:#ffffff10;color:white;transform:translateX(2px)}
    .admin-nav a.on{background:linear-gradient(90deg,#20b99d,#16947f);border-color:#ffffff18;color:white;box-shadow:0 8px 18px #002c2d3b}
    .sidebar-footer{margin-top:auto;border-top:1px solid #ffffff15;padding-top:12px}
    .server-state{font-size:8px;color:#9fc7c1;display:flex;align-items:center;gap:6px;padding:0 6px 8px}.server-state span,.live-dot{width:7px;height:7px;border-radius:50%;background:#2bd978;box-shadow:0 0 0 4px #2bd97818}
    .logout{width:100%;padding:9px;border:0;border-radius:9px;background:#ffffff0d;color:#d9ece8;text-align:left;cursor:pointer}

    .admin-content{min-width:0;position:relative;background:#fafffe33}
    .admin-content:before{content:"";position:fixed;left:230px;right:0;top:0;bottom:0;pointer-events:none;background:rgba(250,255,254,.28);z-index:0}
    .admin-topbar{height:64px;position:sticky;top:0;z-index:45;display:flex;align-items:center;gap:12px;padding:0 20px;background:#ffffffc4;backdrop-filter:blur(18px);border-bottom:1px solid #a6d9cf91}
    .hamb{display:none;width:36px;height:36px;border:1px solid #acd8cf;border-radius:9px;background:white}
    .top-title{display:flex;flex-direction:column}.top-title span{font-size:8px;letter-spacing:2px;font-weight:950;color:#13937e}.top-title b{font-size:12px;color:#12363b}.top-spacer{flex:1}
    .admin-user{display:flex;align-items:center;gap:9px;padding:5px 6px 5px 10px;border:1px solid #acd9d0;border-radius:11px;background:#ffffffdc}
    .admin-user>div{display:flex;flex-direction:column}.admin-user small{font-size:7px;color:#778d91}.admin-user strong{font-size:9px}.admin-user i{width:31px;height:31px;border-radius:50%;display:grid;place-items:center;background:linear-gradient(135deg,#07383a,#168b78);color:white;font-size:10px;font-style:normal;font-weight:950}

    .quick-nav{position:sticky;top:64px;z-index:40;display:flex;align-items:center;gap:7px;padding:9px 18px;background:#f7fffdde;backdrop-filter:blur(14px);border-bottom:1px solid #a9dcd2;overflow:auto}
    .quick-title{font-size:9px;font-weight:950;text-transform:uppercase;letter-spacing:1px;color:#668087;margin-right:3px;white-space:nowrap}
    .quick-nav a{white-space:nowrap;text-decoration:none;color:#29464c;padding:8px 12px;border-radius:9px;border:1px solid #98d2c7;background:#ffffffd9;font-size:10px;box-shadow:0 3px 9px #0b5b5c0a;transition:.15s}
    .quick-nav a b{font-weight:900}.quick-nav a:hover,.quick-nav a.active{background:linear-gradient(135deg,#0b6d64,#18a88e);color:white;border-color:#0d8f7b;transform:translateY(-1px)}

    .admin-page{position:relative;z-index:1;min-height:calc(100vh - 110px)}
    .admin-layout .pagex{padding:24px!important;background:transparent!important}
    .admin-layout .head{display:flex!important;justify-content:space-between!important;align-items:center!important;gap:14px!important;margin-bottom:16px!important}
    .admin-layout .head h1{font-size:32px!important;line-height:1.1!important;margin:0!important;color:#123a3f!important;font-weight:900!important;letter-spacing:-.7px}
    .admin-layout .head p{margin:5px 0 0!important;color:#647d82!important;font-size:10px!important}
    .admin-layout .card{background:#ffffffdc!important;backdrop-filter:blur(15px)!important;border:1.5px solid #99d2c7!important;border-radius:15px!important;box-shadow:0 10px 26px #0b5c5b12!important}
    .admin-layout .panel{padding:17px!important}
    .admin-layout .card:hover{border-color:#57b8a7!important}
    .admin-layout .input{min-height:43px!important;border:1.4px solid #a2d5cb!important;border-radius:9px!important;background:#fffffff0!important;color:#18373c!important;padding:0 12px!important;outline:0!important;box-sizing:border-box;font-size:12px!important}
    .admin-layout .input:focus{border-color:#16a187!important;box-shadow:0 0 0 3px #18a88e18!important}
    .admin-layout .btn{min-height:42px!important;border-radius:9px!important;padding:0 14px!important;font-size:11px!important;font-weight:900!important;cursor:pointer!important}
    .admin-layout .btn.primary{background:linear-gradient(135deg,#0a5f5b,#18a88e)!important;color:white!important;border:1px solid #0b8474!important}
    .admin-layout .small{font-weight:850!important;border-radius:8px!important;border:1px solid transparent!important}
    .admin-layout table{background:#ffffffb8!important;border:1px solid #a6d8ce!important;border-radius:12px!important;overflow:hidden!important}
    .admin-layout th{background:#edf9f6e8!important;color:#587177!important;font-size:10px!important;font-weight:950!important;text-transform:uppercase!important;letter-spacing:.6px!important}
    .admin-layout td{background:#ffffff9e!important;border-bottom:1px solid #d9ebe7!important;font-size:11px!important}
    .admin-layout h2,.admin-layout h3{color:#163b40!important}


    /* Minor readability polish across every Admin page */
    .admin-layout .pagex h2{font-size:18px!important;font-weight:900!important}
    .admin-layout .pagex h3{font-size:17px!important;font-weight:900!important}
    .admin-layout .pagex label{font-size:11px!important;font-weight:850!important}
    .admin-layout .pagex .card p{font-size:10px!important;line-height:1.55!important}
    .admin-layout .pagex .card small{font-size:9px!important}
    .admin-layout table{border-width:1.7px!important;border-color:#72c4b5!important}
    .admin-layout th{border-bottom:1.5px solid #a7d8cf!important}
    .admin-layout td b{font-size:11px!important}
    .admin-layout td small{font-size:9px!important}
    .admin-layout .card{border-width:1.7px!important}


    /* v3.2 - GLOBAL ADMIN FONT UPGRADE
       Applies only inside Admin pages and overrides tiny component-level fonts. */
    .admin-layout .nav-label{font-size:9px!important}
    .admin-layout .admin-nav a{font-size:12px!important}
    .admin-layout .admin-nav a b{font-size:12px!important}
    .admin-layout .server-state{font-size:10px!important}
    .admin-layout .logout{font-size:11px!important}
    .admin-layout .admin-brand strong{font-size:16px!important}
    .admin-layout .admin-brand span{font-size:9px!important}

    .admin-layout .top-title span{font-size:10px!important}
    .admin-layout .top-title b{font-size:14px!important}
    .admin-layout .admin-user small{font-size:9px!important}
    .admin-layout .admin-user strong{font-size:11px!important}

    .admin-layout .quick-title{font-size:10px!important}
    .admin-layout .quick-nav a{font-size:11px!important;padding:9px 13px!important}

    .admin-layout .pagex .eyebrow{font-size:10px!important}
    .admin-layout .pagex .head h1{font-size:34px!important}
    .admin-layout .pagex .head p{font-size:14px!important;line-height:1.6!important;font-weight:500!important}
    .admin-layout .pagex h2{font-size:20px!important}
    .admin-layout .pagex h3{font-size:18px!important}

    .admin-layout .pagex label{font-size:12px!important}
    .admin-layout .pagex .input{font-size:12px!important}
    .admin-layout .pagex .btn{font-size:11px!important}
    .admin-layout .pagex .small{font-size:10px!important}

    .admin-layout .pagex .card p{font-size:11px!important}
    .admin-layout .pagex .card small{font-size:10px!important}
    .admin-layout .pagex .card span{font-size:10px!important}
    .admin-layout .pagex .card b{font-size:12px}

    .admin-layout table th{font-size:11px!important}
    .admin-layout table td{font-size:12px!important}
    .admin-layout table td b{font-size:12px!important}
    .admin-layout table td small{font-size:10px!important}

    .admin-layout .status-chip,
    .admin-layout .payment-chip,
    .admin-layout .parking-chip,
    .admin-layout .ticket-count,
    .admin-layout .active-chip,
    .admin-layout .id-chip{
      font-size:10px!important;
    }

    .admin-layout .summary span,
    .admin-layout .summary small{font-size:11px!important}
    .admin-layout .summary b{font-size:25px!important}

    .admin-layout .kpi small,
    .admin-layout .kpi span{font-size:10px!important}
    .admin-layout .kpi b{font-size:22px!important}

    .admin-layout .setting-head b{font-size:14px!important}
    .admin-layout .setting-head small{font-size:10px!important}
    .admin-layout .toggle-row b{font-size:12px!important}
    .admin-layout .toggle-row small{font-size:10px!important}
    .admin-layout .field > small{font-size:10px!important}

    /* Keep dense seat/parking maps usable while still more readable */
    .admin-layout .seat b,
    .admin-layout .parking-slot b{font-size:10px!important}
    .admin-layout .seat small,
    .admin-layout .parking-slot small,
    .admin-layout .seat em,
    .admin-layout .parking-slot em{font-size:8px!important}

    .sidebar-backdrop{display:none}
    @media(max-width:850px){
      .admin-layout{grid-template-columns:1fr}.admin-sidebar{position:fixed;left:-250px;width:230px;transition:.2s}.admin-sidebar.open{left:0}.sidebar-close{display:block}
      .sidebar-backdrop{display:block;position:fixed;inset:0;z-index:55;background:#062f315c}.hamb{display:grid;place-items:center}.admin-content:before{left:0}
      .quick-nav{padding-left:12px}.admin-layout .pagex{padding:16px!important}
    }
    @media(max-width:560px){.top-title{display:none}.admin-user small{display:none}.admin-layout .head{align-items:flex-start!important;flex-direction:column!important}}
  `]
})
export class AdminShellComponent{
  readonly auth=inject(AuthService);
  readonly open=signal(false);
  initial(){return String(this.auth.user()?.fullName||'A').trim().charAt(0).toUpperCase()}
}
