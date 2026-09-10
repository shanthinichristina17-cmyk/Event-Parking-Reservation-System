import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CustomerApiService } from '../customer-api.service';

@Component({
  selector:'app-home',
  imports:[RouterLink],
  template:`
    <section class="hero">
      <div class="page hero-grid">
        <div class="hero-copy">
          <span class="eyebrow"><i></i> SMART EVENT + PARKING EXPERIENCE</span>
          <h1>Book the moment.<br><em>Own the seat.</em><br>Park with confidence.</h1>
          <p>Explore events, choose live seats, reserve optional parking and keep your complete booking journey in one customer account.</p>

          <div class="hero-actions">
            <a class="primary" routerLink="/events">Explore live events <span>→</span></a>
            <a class="secondary" routerLink="/register">Create account</a>
          </div>

          <div class="trust-row">
            <article><b>LIVE</b><span>Seat availability</span></article>
            <article><b>5 MIN</b><span>Protected booking hold</span></article>
            <article><b>QR</b><span>Digital ticket</span></article>
          </div>
        </div>

        <div class="hero-visual">
          <img src="/customer-assets/home-event-parking.jpg" alt="Event venue parking and booking experience">
        </div>
      </div>
    </section>

    <section class="page journey-strip">
      <article><span>01</span><div><b>Discover</b><small>Find an event that matches your mood.</small></div></article>
      <article><span>02</span><div><b>Select</b><small>Pick exact live seats and optional parking.</small></div></article>
      <article><span>03</span><div><b>Checkout</b><small>Complete the protected booking flow.</small></div></article>
      <article><span>04</span><div><b>Enjoy</b><small>Keep your QR ticket ready for the event.</small></div></article>
    </section>

    <section class="page categories">
      <div class="section-head">
        <div><span class="kicker">EXPLORE BY CATEGORY</span><h2>Six ways to find your next event.</h2><p>Choose a category and jump straight into live EventPark inventory.</p></div>
        <a routerLink="/events">View all events →</a>
      </div>

      <div class="category-grid">
        @for(c of categoryCards; track c.name){
          <a class="category-card" routerLink="/events">
            <img [src]="c.image" [alt]="c.name">
            <div><span>{{c.tag}}</span><b>{{c.name}}</b><small>{{c.copy}}</small></div>
          </a>
        }
      </div>
    </section>

    <section class="page live-events">
      <div class="section-head">
        <div><span class="kicker">LIVE INVENTORY</span><h2>Upcoming experiences</h2><p>Events loaded directly from your connected backend.</p></div>
        <a routerLink="/events">Browse everything →</a>
      </div>

      <div class="event-grid">
        @for(e of items();track e.eventId){
          <article class="event-card">
            <div class="event-image">
              <img [src]="categoryImage(e)" [alt]="e.name || 'Event'">
              <span class="live-badge">● LIVE</span>
              <span class="category-badge">{{e.categoryName || 'Event'}}</span>
            </div>
            <div class="event-body">
              <h3>{{e.name}}</h3>
              <div class="meta"><span>◷ {{e.eventDate}} · {{e.startTime}}</span><span>⌖ {{e.venueName || 'Venue'}}</span></div>
              <div class="event-footer">
                <div><small>Tickets from</small><b>Rs. {{e.ticketPrice}}</b></div>
                <a [routerLink]="['/booking',e.eventId]">Choose seats →</a>
              </div>
            </div>
          </article>
        } @empty {
          <div class="empty-state">
            <b>No live events yet.</b>
            <p>Once Admin creates events, they will appear here automatically.</p>
            <a routerLink="/events">Open Explore Events →</a>
          </div>
        }
      </div>
    </section>

    <section class="page smart-panel">
      <div>
        <span class="kicker light">WHY EVENTPARK</span>
        <h2>A booking experience designed around the whole event journey.</h2>
        <p>Instead of separating tickets and parking, EventPark keeps seat availability, parking choice, protected checkout, notifications and tickets together.</p>
      </div>
      <div class="smart-grid">
        <article><span>◎</span><b>Live seat map</b><small>Available, held and booked states stay clear.</small></article>
        <article><span>▦</span><b>Parking in flow</b><small>Add a parking slot without leaving the booking journey.</small></article>
        <article><span>◉</span><b>Account updates</b><small>Booking and payment notifications in one place.</small></article>
        <article><span>QR</span><b>Digital ticket</b><small>Your confirmed booking ends with a QR ticket.</small></article>
      </div>
    </section>
  `,
  styles:[`
    .hero{padding:66px 0 48px;background:linear-gradient(120deg,rgba(238,250,247,.88),rgba(255,255,255,.60),rgba(245,251,248,.78))}
    .hero-grid{display:grid;grid-template-columns:.9fr 1.1fr;gap:38px;align-items:center}
    .eyebrow{display:inline-flex;align-items:center;gap:8px;padding:8px 11px;background:#ffffffdc;border:1px solid #aad9d0;border-radius:999px;font-size:12px;font-weight:900;color:#47656a}.eyebrow i{width:8px;height:8px;border-radius:50%;background:#e6aa22;box-shadow:0 0 0 5px #e6aa221c}
    h1{font-size:clamp(48px,6vw,77px);line-height:.94;letter-spacing:-3.6px;margin:23px 0;color:#07383a}h1 em{font-style:normal;color:#0e8f79}
    .hero-copy>p{font-size:18px;line-height:1.75;color:#5f777c;max-width:610px}
    .hero-actions{display:flex;gap:9px;margin:26px 0 31px}.hero-actions a{min-height:48px;padding:0 19px;border-radius:12px;display:inline-flex;align-items:center;gap:10px;text-decoration:none;font-size:12px;font-weight:900}.primary{background:linear-gradient(135deg,#07383a,#0e8f79);color:#fff;box-shadow:0 13px 28px #07383a20}.primary span{color:#efb42e;font-size:18px}.secondary{background:#ffffffdb;border:1px solid #afd9d1;color:#07383a}
    .trust-row{display:flex;gap:25px;flex-wrap:wrap}.trust-row article{display:flex;flex-direction:column}.trust-row b{font-size:18px;color:#0e8f79}.trust-row span{font-size:11px;color:#71858a}

    .hero-visual{position:relative;min-width:0;border-radius:24px;overflow:hidden;background:#eef7f3;border:1px solid #b9ddd5;box-shadow:0 22px 48px #07383a16}
    .hero-visual img{width:100%;height:100%;min-height:430px;display:block;object-fit:cover}
    

    .journey-strip{display:grid;grid-template-columns:repeat(4,1fr);gap:11px;margin-top:28px}.journey-strip article{padding:17px;background:#ffffffdc;border:1px solid #a8d7ce;border-radius:14px;display:flex;gap:12px;align-items:center;box-shadow:0 12px 28px #07383a08}.journey-strip article>span{width:37px;height:37px;border-radius:10px;background:#e7f7f3;color:#0c8472;display:grid;place-items:center;font-size:9px;font-weight:950}.journey-strip div{display:flex;flex-direction:column}.journey-strip b{font-size:15px}.journey-strip small{font-size:11px;color:#71858a;margin-top:2px;line-height:1.45}

    .categories,.live-events{padding-top:70px}.section-head{display:flex;justify-content:space-between;align-items:end;gap:20px;margin-bottom:18px}.kicker{font-size:11px;letter-spacing:1.8px;font-weight:950;color:#0e8f79}.section-head h2,.smart-panel h2{font-size:clamp(31px,4vw,44px);letter-spacing:-1.4px;margin:6px 0;color:#07383a}.section-head p{margin:0;color:#71858a;font-size:15px}.section-head>a{color:#0e8f79;text-decoration:none;font-size:13px;font-weight:900}
    .category-grid{display:grid;grid-template-columns:repeat(3,1fr);gap:14px}.category-card{position:relative;min-height:205px;border-radius:19px;overflow:hidden;border:1px solid #9fd4ca;box-shadow:0 16px 36px #07383a11;text-decoration:none}.category-card img{width:100%;height:100%;object-fit:cover;position:absolute;inset:0;transition:.3s}.category-card:after{content:"";position:absolute;inset:0;background:linear-gradient(180deg,transparent 33%,#041f25d9 100%)}.category-card:hover img{transform:scale(1.04)}.category-card>div{position:absolute;z-index:1;left:15px;right:15px;bottom:14px;color:#fff;display:flex;flex-direction:column}.category-card span{font-size:9px;letter-spacing:1.2px;color:#efb42e;font-weight:950}.category-card b{font-size:23px}.category-card small{font-size:10px;color:#d1e1de;margin-top:2px}

    .event-grid{display:grid;grid-template-columns:repeat(3,1fr);gap:15px}.event-card{background:#ffffffeb;border:1px solid #9fd4ca;border-radius:18px;overflow:hidden;box-shadow:0 14px 36px #07383a0c}.event-image{height:190px;position:relative;overflow:hidden}.event-image img{width:100%;height:100%;object-fit:cover;transition:.3s}.event-card:hover .event-image img{transform:scale(1.035)}.live-badge,.category-badge{position:absolute;top:12px;padding:6px 8px;border-radius:999px;font-size:8px;font-weight:950}.live-badge{right:12px;background:#07383adb;color:#fff}.category-badge{left:12px;background:#ffffffe8;color:#07383a}.event-body{padding:16px}.event-body h3{margin:0 0 10px;font-size:20px;color:#17343a}.meta{display:flex;flex-direction:column;gap:6px;color:#72878b;font-size:12px}.event-footer{display:flex;justify-content:space-between;align-items:end;margin-top:15px;padding-top:13px;border-top:1px solid #dceae7}.event-footer div{display:flex;flex-direction:column}.event-footer small{font-size:10px;color:#87979a}.event-footer b{font-size:20px;color:#0e8f79}.event-footer a{font-size:12px;font-weight:950;color:#07383a;text-decoration:none}.empty-state{grid-column:1/-1;padding:35px;text-align:center;border:1px dashed #9fcfc6;border-radius:18px;background:#ffffffd8}.empty-state p{color:#74898d}.empty-state a{color:#0e8f79;font-weight:900;text-decoration:none}

    .smart-panel{margin-top:70px;padding:35px;border-radius:23px;background:linear-gradient(135deg,#07383a,#07554f);color:#fff;display:grid;grid-template-columns:.82fr 1.18fr;gap:30px;box-shadow:0 22px 50px #07383a1c}.smart-panel h2{color:#fff}.smart-panel>div:first-child p{color:#bbd4cf;line-height:1.7}.light{color:#efb42e}.smart-grid{display:grid;grid-template-columns:1fr 1fr;gap:10px}.smart-grid article{padding:16px;border:1px solid #ffffff1c;background:#ffffff0b;border-radius:14px;display:flex;flex-direction:column}.smart-grid article>span{width:34px;height:34px;border-radius:10px;background:#ffffff12;display:grid;place-items:center;color:#efb42e;font-weight:950}.smart-grid b{margin:8px 0 3px;font-size:14px}.smart-grid small{color:#a9cac3;font-size:10px;line-height:1.5}
    @media(max-width:950px){.hero-grid,.smart-panel{grid-template-columns:1fr}.hero-visual{order:-1}.category-grid,.event-grid{grid-template-columns:1fr 1fr}.journey-strip{grid-template-columns:1fr 1fr}}
    @media(max-width:590px){.hero-visual img{min-height:280px}.section-head{align-items:flex-start;flex-direction:column}.category-grid,.event-grid,.journey-strip,.smart-grid{grid-template-columns:1fr}.hero-actions{flex-direction:column}.hero-actions a{justify-content:center}}
  `]
})
export class HomeComponent{
  private api=inject(CustomerApiService);
  readonly items=signal<any[]>([]);
  readonly categoryCards=[
    {name:'Concert',tag:'LIVE MUSIC',copy:'Big nights. Easy parking.',image:'/customer-assets/categories/concert.jpg'},
    {name:'Conference',tag:'IDEAS & NETWORK',copy:'Connect, learn and grow.',image:'/customer-assets/categories/conference.jpg'},
    {name:'Seminar',tag:'LEARN',copy:'Focused learning experiences.',image:'/customer-assets/categories/seminar.jpg'},
    {name:'Sports',tag:'MATCH DAY',copy:'Play, compete and celebrate.',image:'/customer-assets/categories/sports.jpg'},
    {name:'Webinar',tag:'ONLINE',copy:'Join and learn from anywhere.',image:'/customer-assets/categories/webinar.jpg'},
    {name:'Workshop',tag:'CREATE',copy:'Hands-on learning and collaboration.',image:'/customer-assets/categories/workshop.jpg'},
  ];

  constructor(){
    this.api.events({page:1,pageSize:6}).subscribe({
      next:(r:any)=>this.items.set((r?.items??r?.data??r??[]).slice(0,6)),
      error:()=>this.items.set([])
    })
  }

  categoryImage(e:any){
    const t=`${e?.categoryName??''} ${e?.name??''}`.toLowerCase();
    if(t.includes('conference')||t.includes('summit'))return '/customer-assets/categories/conference.jpg';
    if(t.includes('seminar'))return '/customer-assets/categories/seminar.jpg';
    if(t.includes('sport')||t.includes('match')||t.includes('final'))return '/customer-assets/categories/sports.jpg';
    if(t.includes('webinar')||t.includes('online'))return '/customer-assets/categories/webinar.jpg';
    if(t.includes('workshop')||t.includes('training')||t.includes('bootcamp'))return '/customer-assets/categories/workshop.jpg';
    return '/customer-assets/categories/concert.jpg';
  }
}
