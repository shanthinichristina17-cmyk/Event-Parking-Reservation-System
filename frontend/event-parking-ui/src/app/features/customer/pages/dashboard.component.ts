import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { CustomerApiService } from '../customer-api.service';

@Component({
  selector: 'app-dashboard',
  imports: [RouterLink],
  template: `
    <main class="page dash">
      <div class="head">
        <div>
          <span class="kicker">CUSTOMER DASHBOARD</span>
          <h1>Welcome back, {{ auth.user()?.fullName || 'Customer' }} 👋</h1>
          <p>Your bookings, spending and notifications in one place.</p>
        </div>
        <a class="browse" routerLink="/events">Browse events →</a>
      </div>

      @if (error()) {
        <div class="notice"><b>Backend connection needed.</b><span>{{ error() }}</span></div>
      }

      <div class="stats">
        <article><span>Active tickets</span><b>{{ d()?.upcomingBookings || 0 }}</b></article>
        <article><span>Total bookings</span><b>{{ d()?.totalBookings || 0 }}</b></article>
        <article><span>Total spent</span><b>Rs. {{ d()?.totalSpent || 0 }}</b></article>
        <article><span>Unread alerts</span><b>{{ d()?.unreadNotifications || 0 }}</b></article>
      </div>

      <div class="grid">
        <section class="panel">
          <h2>Your next booking</h2>
          @if (d()?.nextBooking; as b) {
            <div class="next-booking">
              <span class="badge">CONFIRMED</span>
              <h3>{{ b.eventName }}</h3>
              <p>◷ {{ b.eventDate }} · {{ b.startTime }}</p>
              <p>⌖ {{ b.venueName }}</p>
            </div>
          } @else {
            <div class="empty"><b>No upcoming booking yet.</b><p>Choose an event to start your first reservation.</p><a routerLink="/events">Explore events</a></div>
          }
        </section>

        <section class="panel">
          <h2>Booking summary</h2>
          <div class="summary"><span>Confirmed <b>{{ d()?.confirmedBookings || 0 }}</b></span><span>Cancelled <b>{{ d()?.cancelledBookings || 0 }}</b></span><span>Promo savings <b>Rs. {{ d()?.promoSavings || 0 }}</b></span></div>
        </section>
      </div>
    </main>
  `,
  styles: [`
    .dash{padding:42px 0}.head{display:flex;justify-content:space-between;align-items:center;gap:20px}.kicker{font-size:9px;letter-spacing:2px;font-weight:900;color:#0b7a69}.head h1{font-size:36px;letter-spacing:-1.3px;margin:7px 0;color:#07363a}.head p,.panel p{color:#718184}.browse{padding:12px 16px;border-radius:12px;background:#07363a;color:#fff;font-weight:900;font-size:12px}.notice{margin-top:18px;padding:13px 15px;border:1px solid #f2d49a;background:#fff8e8;border-radius:13px;color:#7c5710;display:flex;gap:8px;flex-wrap:wrap;font-size:12px}.stats{display:grid;grid-template-columns:repeat(4,1fr);gap:13px;margin:24px 0}.stats article,.panel{background:#fff;border:1px solid #dce7e4;border-radius:18px;box-shadow:0 14px 36px #07363a0c}.stats article{padding:19px;display:flex;flex-direction:column;gap:8px}.stats span{font-size:11px;color:#718184}.stats b{font-size:27px;color:#0b7a69}.grid{display:grid;grid-template-columns:1.5fr 1fr;gap:15px}.panel{padding:22px}.panel h2{margin-top:0;color:#07363a}.badge{display:inline-block;font-size:8px;letter-spacing:1px;background:#e5f7f2;color:#0b7a69;padding:6px 8px;border-radius:999px;font-weight:900}.next-booking h3{font-size:22px;margin:12px 0}.summary{display:flex;flex-direction:column;gap:13px}.summary span{display:flex;justify-content:space-between;color:#718184;padding-bottom:10px;border-bottom:1px solid #edf2f0}.summary b{color:#07363a}.empty{padding:20px;background:#f7faf8;border-radius:14px}.empty p{margin-bottom:14px}.empty a{color:#0b7a69;font-weight:900;font-size:12px}@media(max-width:750px){.head{align-items:flex-start;flex-direction:column}.stats{grid-template-columns:1fr 1fr}.grid{grid-template-columns:1fr}}@media(max-width:440px){.stats{grid-template-columns:1fr}}
  `]
})
export class DashboardComponent {
  readonly auth=inject(AuthService);
  private api=inject(CustomerApiService);
  readonly d=signal<any>({});
  readonly error=signal('');
  constructor(){
    this.api.dashboard().subscribe({
      next:x=>{this.d.set(x||{});this.error.set('')},
      error:()=>this.error.set('Start the API at http://localhost:5118, then refresh this page.')
    });
  }
}
