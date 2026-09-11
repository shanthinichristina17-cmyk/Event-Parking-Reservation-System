import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CustomerApiService } from '../customer-api.service';

@Component({
  selector:'app-my-bookings',
  imports:[RouterLink],
  template:`
    <main class="page wrap">
      <div class="head">
        <div><span class="kicker">MY EVENTPARK</span><h1>My bookings</h1><p>Track tickets, parking, payment status and booking history.</p></div>
        <a routerLink="/events">Book another event →</a>
      </div>

      <div class="tabs">
        <button [class.active]="tab()==='upcoming'" (click)="load('upcoming')">Upcoming</button>
        <button [class.active]="tab()==='past'" (click)="load('past')">Past</button>
        <button [class.active]="tab()==='cancelled'" (click)="load('cancelled')">Cancelled</button>
      </div>

      @if(error()){<div class="notice">{{error()}}</div>}

      <div class="booking-list">
        @for(b of items();track b.bookingId){
          <article class="booking">
            <div class="booking-mark"><span>EP</span><small>#{{b.bookingId}}</small></div>
            <div class="details">
              <div class="title-row"><h3>{{b.eventName}}</h3><span class="status" [class.confirmed]="b.status==='Confirmed'" [class.cancelled]="b.status==='Cancelled'" [class.expired]="b.status==='Expired'">{{b.status}}</span></div>
              <p>◷ {{b.eventDate}} · {{b.startTime}}</p>
              <p>⌖ {{b.venueName}}</p>
              <div class="meta-row"><span>Booking <b>{{b.bookingNumber}}</b></span>@if(b.seatCount!=null){<span>Tickets <b>{{b.seatCount}}</b></span>}@if(b.parkingSlot){<span>Parking <b>{{b.parkingSlot}}</b></span>}</div>
            </div>
            <div class="right">
              <small>Total</small><b>Rs. {{b.finalTotal}}</b>
              @if(b.status==='Confirmed'){<button (click)="cancel(b.bookingId)">Cancel booking</button>}
            </div>
          </article>
        } @empty {
          <section class="empty"><div>◎</div><b>No bookings in this list.</b><p>When you reserve an event, it will appear here with ticket and parking details.</p><a routerLink="/events">Browse events →</a></section>
        }
      </div>
    </main>
  `,
  styles:[`
    .wrap{padding:42px 0}.head{display:flex;justify-content:space-between;align-items:center;gap:18px}.kicker{font-size:9px;letter-spacing:1.8px;color:#0e8f79;font-weight:950}.head h1{font-size:39px;letter-spacing:-1.4px;margin:6px 0;color:#07383a}.head p{margin:0;color:#6c8287}.head>a{padding:10px 13px;border-radius:10px;background:#07383a;color:#fff;text-decoration:none;font-size:9px;font-weight:950}
    .tabs{display:flex;gap:7px;margin:21px 0}.tabs button{border:1px solid #a9d8cf;background:#ffffffdb;border-radius:99px;padding:8px 12px;color:#4a6469;font-size:9px;font-weight:900;cursor:pointer}.tabs button.active{background:#0e8f79;color:#fff;border-color:#0e8f79}
    .booking-list{display:flex;flex-direction:column;gap:10px}.booking{display:grid;grid-template-columns:67px 1fr auto;align-items:center;gap:14px;padding:14px;background:#ffffffeb;border:1px solid #9fd4ca;border-radius:16px;box-shadow:0 12px 32px #07383a0b}.booking-mark{height:66px;border-radius:12px;background:linear-gradient(135deg,#07383a,#0e8f79);color:#fff;display:grid;place-items:center;align-content:center}.booking-mark span{font-weight:950;font-size:14px}.booking-mark small{font-size:7px;color:#b8d7d1}.details h3{font-size:16px;margin:0;color:#17383d}.title-row{display:flex;align-items:center;gap:9px;flex-wrap:wrap}.details>p{margin:4px 0;color:#70858a;font-size:9px}.status{padding:5px 7px;border-radius:99px;background:#fff4d7;color:#9a6500;font-size:7px;font-weight:950}.status.confirmed{background:#e8f7ed;color:#147a3c}.status.cancelled{background:#ffebeb;color:#c02b2b}.status.expired{background:#edf0f1;color:#67747a}
    .meta-row{display:flex;gap:7px;flex-wrap:wrap;margin-top:8px}.meta-row span{padding:6px 8px;border-radius:8px;background:#f1f8f6;border:1px solid #d6ebe6;color:#74878b;font-size:7px}.meta-row b{color:#28494e}
    .right{text-align:right;display:flex;flex-direction:column;gap:4px;align-items:flex-end}.right small{font-size:7px;color:#809196}.right>b{font-size:16px;color:#0e8f79}.right button{margin-top:5px;border:1px solid #efb4b4;background:#fff1f1;color:#b72b2b;border-radius:8px;padding:7px 9px;font-size:7px;font-weight:900;cursor:pointer}
    .empty,.notice{padding:24px;border-radius:16px}.empty{text-align:center;background:#ffffffdb;border:1px dashed #9fcfc6}.empty>div{width:45px;height:45px;border-radius:50%;background:#e7f7f3;color:#0e8f79;display:grid;place-items:center;margin:0 auto 8px}.empty p{color:#71858a}.empty a{color:#0e8f79;text-decoration:none;font-weight:900}.notice{background:#fff8e8;border:1px solid #efcf91;color:#805a13;margin-bottom:12px;font-size:9px}
    @media(max-width:620px){.head{align-items:flex-start;flex-direction:column}.booking{grid-template-columns:55px 1fr}.right{grid-column:1/-1;align-items:flex-start;text-align:left;border-top:1px solid #dceae7;padding-top:10px}}
  `]
})
export class MyBookingsComponent{
  private api=inject(CustomerApiService);
  readonly items=signal<any[]>([]);
  readonly error=signal('');
  readonly tab=signal('upcoming');
  constructor(){this.load('upcoming')}
  load(t:string){this.tab.set(t);this.api.bookings(t).subscribe({next:x=>{this.items.set(x||[]);this.error.set('')},error:()=>{this.items.set([]);this.error.set('Could not load bookings. Make sure the backend is running and you are signed in as a Customer.')}})}
  cancel(id:number){if(confirm('Cancel this booking?'))this.api.cancel(id).subscribe({next:()=>this.load(this.tab()),error:e=>this.error.set(e?.error?.message??'Cancellation failed. Please try again.')})}
}
