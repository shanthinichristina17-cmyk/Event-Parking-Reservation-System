import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { CustomerApiService } from '../customer-api.service';

@Component({
  selector:'app-dashboard',
  imports:[RouterLink],
  template:`
    <main class="page dash">
      <div class="head">
        <div><span class="kicker">CUSTOMER DASHBOARD</span><h1>Welcome back, {{auth.user()?.fullName || 'Customer'}}</h1><p>Your tickets, spending, next event and notifications in one place.</p></div>
        <a class="browse" routerLink="/events">Explore events →</a>
      </div>

      @if(error()){<div class="notice"><b>Dashboard unavailable.</b><span>{{error()}}</span></div>}

      <div class="stats">
        <article class="s1"><span>Active tickets</span><b>{{d()?.upcomingBookings || 0}}</b><small>Upcoming bookings</small></article>
        <article class="s2"><span>Total bookings</span><b>{{d()?.totalBookings || 0}}</b><small>Your EventPark history</small></article>
        <article class="s3"><span>Total spent</span><b>Rs. {{d()?.totalSpent || 0}}</b><small>Completed booking value</small></article>
        <article class="s4"><span>Unread updates</span><b>{{d()?.unreadNotifications || 0}}</b><small>Notification centre</small></article>
      </div>

      <div class="dashboard-grid">
        <section class="panel next-panel">
          <div class="panel-head"><div><span>NEXT EVENT</span><h2>Your next booking</h2></div><a routerLink="/my-bookings">All bookings →</a></div>
          @if(d()?.nextBooking;as b){
            <div class="next-booking">
              <div class="date-block"><b>{{day(b.eventDate)}}</b><span>{{month(b.eventDate)}}</span></div>
              <div class="event-info"><span class="confirmed">● CONFIRMED</span><h3>{{b.eventName}}</h3><p>◷ {{b.eventDate}} · {{b.startTime}}</p><p>⌖ {{b.venueName}}</p></div>
            </div>
          } @else {
            <div class="empty"><b>No upcoming booking yet.</b><p>Choose an event and start your next experience.</p><a routerLink="/events">Find an event</a></div>
          }
        </section>

        <section class="panel summary-panel">
          <div class="panel-head"><div><span>YOUR ACTIVITY</span><h2>Booking summary</h2></div></div>
          <div class="summary">
            <div><span>Confirmed</span><b>{{d()?.confirmedBookings || 0}}</b></div>
            <div><span>Cancelled</span><b>{{d()?.cancelledBookings || 0}}</b></div>
            <div><span>Promo savings</span><b>Rs. {{d()?.promoSavings || 0}}</b></div>
          </div>
        </section>

        <section class="panel quick-panel">
          <div class="panel-head"><div><span>QUICK ACTIONS</span><h2>What do you want to do?</h2></div></div>
          <div class="actions">
            <a routerLink="/events"><i>⌕</i><div><b>Find an event</b><small>Browse live inventory</small></div><span>→</span></a>
            <a routerLink="/my-bookings"><i>▣</i><div><b>Open my tickets</b><small>Bookings and status</small></div><span>→</span></a>
            <a routerLink="/notifications"><i>◉</i><div><b>Check updates</b><small>Payment and booking alerts</small></div><span>→</span></a>
            <a routerLink="/profile"><i>◎</i><div><b>Manage profile</b><small>Name, phone and account email</small></div><span>→</span></a>
          </div>
        </section>

        <section class="panel journey-panel">
          <div class="panel-head"><div><span>EVENTPARK FLOW</span><h2>Your booking journey</h2></div></div>
          <div class="journey"><span class="done"><b>1</b>Event</span><i></i><span class="done"><b>2</b>Seat</span><i></i><span><b>3</b>Parking</span><i></i><span><b>4</b>Ticket</span></div>
          <p>EventPark keeps discovery, seats, parking, checkout and ticket access inside one connected customer experience.</p>
        </section>
      </div>
    </main>
  `,
  styles:[`
    .dash{padding:42px 0}.head{display:flex;justify-content:space-between;align-items:center;gap:20px}.kicker{font-size:9px;letter-spacing:1.8px;font-weight:950;color:#0e8f79}.head h1{font-size:38px;letter-spacing:-1.4px;margin:7px 0;color:#07383a}.head p{color:#687f84;margin:0}.browse{padding:11px 15px;border-radius:10px;background:linear-gradient(135deg,#07383a,#0e8f79);color:#fff;text-decoration:none;font-size:10px;font-weight:950}
    .notice{margin-top:17px;padding:12px 14px;border:1px solid #efc97d;background:#fff8e8;border-radius:12px;color:#805812;display:flex;gap:8px;font-size:10px}
    .stats{display:grid;grid-template-columns:repeat(4,1fr);gap:12px;margin:22px 0}.stats article{position:relative;overflow:hidden;padding:17px;border-radius:16px;color:#fff;display:flex;flex-direction:column;min-height:105px;box-shadow:0 14px 32px #07383a12}.stats article:after{content:"";position:absolute;width:74px;height:74px;border-radius:50%;right:-18px;top:-19px;background:#ffffff12}.stats span{font-size:8px;text-transform:uppercase;letter-spacing:.8px;opacity:.78}.stats b{font-size:25px;margin:5px 0}.stats small{font-size:8px;opacity:.72}.s1{background:linear-gradient(135deg,#0c7266,#18a88e)}.s2{background:linear-gradient(135deg,#2759b8,#3c7ce4)}.s3{background:linear-gradient(135deg,#a96a08,#dea228)}.s4{background:linear-gradient(135deg,#6332aa,#8655ce)}
    .dashboard-grid{display:grid;grid-template-columns:1.25fr .75fr;gap:13px}.panel{padding:19px;background:#ffffffdf;border:1px solid #9fd4ca;border-radius:17px;box-shadow:0 14px 34px #07383a0b}.panel-head{display:flex;justify-content:space-between;align-items:flex-start;margin-bottom:14px}.panel-head span{font-size:7px;color:#0e8f79;letter-spacing:1.4px;font-weight:950}.panel-head h2{font-size:16px;margin:3px 0 0;color:#17383d}.panel-head a{font-size:9px;color:#0e8f79;text-decoration:none;font-weight:900}
    .next-booking{display:grid;grid-template-columns:68px 1fr;gap:14px;align-items:center;padding:15px;background:linear-gradient(135deg,#f1faf7,#fff);border:1px solid #cce9e3;border-radius:14px}.date-block{width:64px;height:70px;border-radius:13px;background:#07383a;color:#fff;display:grid;place-items:center;align-content:center}.date-block b{font-size:22px}.date-block span{font-size:8px;letter-spacing:1px;color:#eab132}.event-info h3{font-size:18px;margin:7px 0}.event-info p{margin:5px 0;color:#6f858a;font-size:9px}.confirmed{display:inline-block;font-size:7px!important;padding:5px 7px;border-radius:99px;background:#e8f7ed;color:#147a3c!important;letter-spacing:.8px!important}
    .summary{display:flex;flex-direction:column;gap:8px}.summary div{padding:11px 12px;border-radius:11px;background:#f5faf9;border:1px solid #d7ebe7;display:flex;justify-content:space-between;align-items:center}.summary span{font-size:9px;color:#6e8388}.summary b{font-size:15px;color:#17383d}
    .quick-panel{grid-column:1/2}.actions{display:grid;grid-template-columns:1fr 1fr;gap:9px}.actions a{display:grid;grid-template-columns:34px 1fr auto;gap:9px;align-items:center;padding:11px;border:1px solid #d1e9e4;border-radius:12px;background:#f9fcfb;text-decoration:none;color:#29464c}.actions i{width:32px;height:32px;border-radius:9px;background:#e8f7f3;color:#0e8f79;display:grid;place-items:center;font-style:normal}.actions div{display:flex;flex-direction:column}.actions b{font-size:9px}.actions small{font-size:7px;color:#768a8f}.actions>a>span{color:#0e8f79}
    .journey-panel p{font-size:8px;color:#71858a;line-height:1.6;margin-bottom:0}.journey{display:flex;align-items:center}.journey span{display:flex;flex-direction:column;align-items:center;gap:4px;font-size:7px;color:#6f8287}.journey b{width:27px;height:27px;border-radius:50%;display:grid;place-items:center;background:#e9f3f1;color:#668085}.journey span.done b{background:#0e8f79;color:#fff}.journey i{height:2px;flex:1;background:#cfe4df;margin:0 5px 14px}.empty{padding:17px;background:#f5faf9;border-radius:13px}.empty p{font-size:9px;color:#71858a}.empty a{color:#0e8f79;text-decoration:none;font-size:9px;font-weight:900}
    @media(max-width:850px){.stats{grid-template-columns:1fr 1fr}.dashboard-grid{grid-template-columns:1fr}.quick-panel{grid-column:auto}}@media(max-width:550px){.head{align-items:flex-start;flex-direction:column}.stats,.actions{grid-template-columns:1fr}}
  `]
})
export class DashboardComponent{
  readonly auth=inject(AuthService);
  private api=inject(CustomerApiService);
  readonly d=signal<any>({});
  readonly error=signal('');
  constructor(){
    this.api.dashboard().subscribe({
      next:x=>{this.d.set(x||{});this.error.set('')},
      error:()=>this.error.set('Make sure the API is running and you are signed in as a Customer.')
    })
  }
  day(v:any){const d=new Date(v);return Number.isNaN(d.getTime())?'--':String(d.getDate()).padStart(2,'0')}
  month(v:any){const d=new Date(v);return Number.isNaN(d.getTime())?'EVENT':d.toLocaleDateString('en-US',{month:'short'}).toUpperCase()}
}
