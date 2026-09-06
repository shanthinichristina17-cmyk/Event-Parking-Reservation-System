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
          <h1>Welcome back, {{ auth.user()?.fullName || 'Customer' }} 👋</h1>
          <p>Here is everything happening across your bookings.</p>
        </div>
        <a class="btn primary" routerLink="/events">Browse events</a>
      </div>

      @if (d(); as x) {
        <div class="stats">
          <article class="card">
            <span>Active tickets</span>
            <b>{{ x.upcomingBookings || 0 }}</b>
          </article>

          <article class="card">
            <span>Total bookings</span>
            <b>{{ x.totalBookings || 0 }}</b>
          </article>

          <article class="card">
            <span>Total spent</span>
            <b>Rs. {{ x.totalSpent || 0 }}</b>
          </article>

          <article class="card">
            <span>Unread alerts</span>
            <b>{{ x.unreadNotifications || 0 }}</b>
          </article>
        </div>

        <div class="grid">
          <section class="card panel">
            <h2>Your next booking</h2>

            @if (x.nextBooking) {
              <h3>{{ x.nextBooking.eventName }}</h3>
              <p>{{ x.nextBooking.eventDate }} · {{ x.nextBooking.startTime }}</p>
              <p>{{ x.nextBooking.venueName }}</p>
            } @else {
              <p>No upcoming booking.</p>
            }
          </section>

          <section class="card panel">
            <h2>Booking summary</h2>
            <p>Confirmed <b>{{ x.confirmedBookings || 0 }}</b></p>
            <p>Cancelled <b>{{ x.cancelledBookings || 0 }}</b></p>
            <p>Promo savings <b>Rs. {{ x.promoSavings || 0 }}</b></p>
          </section>
        </div>
      }
    </main>
  `,
  styles: [`
    .dash { padding: 38px 0; }
    .head { display: flex; justify-content: space-between; align-items: center; gap: 16px; }
    .head h1 { font-size: 34px; margin: 0; }
    .head p, .panel p { color: var(--muted); }

    .stats {
      display: grid;
      grid-template-columns: repeat(4, 1fr);
      gap: 12px;
      margin: 24px 0;
    }

    .stats article, .panel { padding: 18px; }
    .stats article { display: flex; flex-direction: column; gap: 8px; }
    .stats span { color: var(--muted); font-size: 11px; }
    .stats b { font-size: 25px; }

    .grid {
      display: grid;
      grid-template-columns: 1.5fr 1fr;
      gap: 14px;
    }

    @media (max-width: 750px) {
      .stats { grid-template-columns: 1fr 1fr; }
      .grid { grid-template-columns: 1fr; }
    }
  `]
})
export class DashboardComponent {
  readonly auth = inject(AuthService);
  private readonly api = inject(CustomerApiService);
  readonly d = signal<any>(null);

  constructor() {
    this.api.dashboard().subscribe({
      next: x => this.d.set(x),
      error: () => this.d.set({})
    });
  }
}
