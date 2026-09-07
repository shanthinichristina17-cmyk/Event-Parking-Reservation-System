import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CustomerApiService } from '../customer-api.service';

@Component({
  selector:'app-my-bookings',
  imports:[RouterLink],
  template:`
    <main class="page wrap">
      <span class="kicker">MY EVENTPARK</span><h1>My bookings</h1><p>Tickets, parking and booking status in one place.</p>
      <div class="tabs"><button (click)="load('upcoming')">Upcoming</button><button (click)="load('past')">Past</button><button (click)="load('cancelled')">Cancelled</button></div>
      @if(error()){<div class="notice">{{error()}}</div>}
      @for(b of items();track b.bookingId){
        <article class="booking">
          <div class="thumb">EP</div>
          <div><h3>{{b.eventName}}</h3><p>{{b.eventDate}} · {{b.startTime}} · {{b.venueName}}</p><small>{{b.bookingNumber}}</small></div>
          <div class="right"><span>{{b.status}}</span><b>Rs. {{b.finalTotal}}</b>@if(b.status==='Confirmed'){<button (click)="cancel(b.bookingId)">Cancel</button>}</div>
        </article>
      } @empty {
        <section class="empty"><b>No bookings in this list.</b><p>Once you reserve an event, it will appear here.</p><a routerLink="/events">Browse events →</a></section>
      }
    </main>`,
  styles:[`
    .wrap{padding:42px 0}.kicker{font-size:9px;letter-spacing:2px;color:#0b7a69;font-weight:900}.wrap h1{font-size:40px;letter-spacing:-1.5px;margin:7px 0;color:#07363a}.wrap>p,.booking p,.booking small,.empty p{color:#718184}.tabs{display:flex;gap:7px;margin:22px 0}.tabs button{border:1px solid #dce7e4;background:#fff;border-radius:99px;padding:9px 12px;cursor:pointer}.booking{display:grid;grid-template-columns:75px 1fr auto;align-items:center;gap:14px;padding:14px;margin-bottom:12px;background:#fff;border:1px solid #dce7e4;border-radius:17px;box-shadow:0 12px 32px #07363a0b}.thumb{height:65px;border-radius:11px;background:linear-gradient(135deg,#07363a,#0b7a69);color:#fff;display:grid;place-items:center;font-weight:900}.booking h3{margin:0;color:#07363a}.right{text-align:right;display:flex;flex-direction:column;gap:5px}.right span{color:#15803d;font-size:11px}.right b{color:#0b7a69}.right button{border:0;background:transparent;color:#c2410c;cursor:pointer}.empty,.notice{padding:22px;border-radius:16px}.empty{background:#fff;border:1px dashed #cddbd8}.empty a{color:#0b7a69;font-weight:900}.notice{background:#fff8e8;border:1px solid #f2d49a;color:#7c5710;margin-bottom:14px}@media(max-width:550px){.booking{grid-template-columns:60px 1fr}.right{grid-column:1/-1;text-align:left}}
  `]
})
export class MyBookingsComponent{
  private api=inject(CustomerApiService);
  readonly items=signal<any[]>([]);
  readonly error=signal('');
  constructor(){this.load('upcoming')}
  load(t:string){this.api.bookings(t).subscribe({next:x=>{this.items.set(x||[]);this.error.set('')},error:()=>{this.items.set([]);this.error.set('Could not load bookings. Make sure the backend is running and you are signed in as a Customer.')}})}
  cancel(id:number){if(confirm('Cancel booking?'))this.api.cancel(id).subscribe({next:()=>this.load('upcoming'),error:()=>this.error.set('Cancellation failed. Please try again.')})}
}
