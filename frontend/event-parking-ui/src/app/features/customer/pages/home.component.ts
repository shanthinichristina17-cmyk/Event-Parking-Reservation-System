import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CustomerApiService } from '../customer-api.service';

@Component({
  selector:'app-home',
  imports:[RouterLink],
  template:`
    <section class="hero">
      <div class="page hero-grid">
        <div class="copy">
          <span class="eyebrow"><i></i> Smart event booking platform</span>
          <h1>Your event.<br><em>Your seat.</em><br>Your parking.</h1>
          <p>Discover experiences, choose exact seats, add optional parking and complete checkout in one connected web application.</p>

          <div class="hero-actions">
            <a class="hero-btn primary-hero" routerLink="/events">Explore events <span>→</span></a>
            <a class="hero-btn ghost-hero" routerLink="/register">Create free account</a>
          </div>

          <div class="trust">
            <div><b>5 min</b><span>Protected seat hold</span></div>
            <div><b>1 flow</b><span>Seat + parking</span></div>
            <div><b>QR</b><span>Instant ticket</span></div>
          </div>
        </div>

        <div class="hero-visual" aria-label="Event parking experience">
          <img src="/customer-assets/home-event-parking.jpg" alt="Event venue parking, reserved parking and festival booking experience" (error)="heroFallback($event)">
        </div>
      </div>
    </section>

    <section class="page feature-strip">
      <article><span>01</span><div><b>Pick your exact seat</b><small>See live seat availability before you book.</small></div></article>
      <article><span>02</span><div><b>Add parking if needed</b><small>Choose one available event parking slot.</small></div></article>
      <article><span>03</span><div><b>Checkout with confidence</b><small>Your selected resources stay protected during the hold.</small></div></article>
    </section>

    <section class="page events-section">
      <div class="section-head">
        <div><span class="kicker">DISCOVER</span><h2>Upcoming experiences</h2><p>Live events available from the connected backend.</p></div>
        <a routerLink="/events">Browse all events →</a>
      </div>

      <div class="event-grid">
        @for(e of items();track e.eventId){
          <article class="event-card">
            <div class="image-wrap">
              <img [src]="eventImage(e)" [alt]="e.name || 'Event image'" (error)="eventFallback($event,e)">
              <span class="category">{{e.categoryName || 'Event'}}</span>
              <span class="availability">● Live availability</span>
            </div>

            <div class="event-content">
              <h3>{{e.name}}</h3>
              <p>◷ {{e.eventDate}} · {{e.startTime}}</p>
              <p>⌖ {{e.venueName || 'Venue'}}</p>
              <div class="event-bottom">
                <div><small>Starting from</small><strong>Rs. {{e.ticketPrice}}</strong></div>
                <a [routerLink]="['/booking',e.eventId]">Book now →</a>
              </div>
            </div>
          </article>
        } @empty {
          <div class="empty-state">
            <div class="empty-copy">
              <span class="kicker">BACKEND READY</span>
              <b>No live events yet</b>
              <p>When the API is running and an Admin creates events, real event cards will replace these previews automatically.</p>
              <a routerLink="/events">Open events page →</a>
            </div>
            <div class="preview-events" aria-label="Event image previews">
              <article><img src="/customer-assets/event-music.jpg" alt="Concert preview" (error)="staticFallback($event,'/customer-assets/event-music.svg')"><span>Concert</span></article>
              <article><img src="/customer-assets/event-sports.jpg" alt="Sports event preview" (error)="staticFallback($event,'/customer-assets/event-sports.svg')"><span>Sports</span></article>
              <article><img src="/customer-assets/event-workshop.jpg" alt="Workshop preview" (error)="staticFallback($event,'/customer-assets/event-workshop.svg')"><span>Workshop</span></article>
            </div>
          </div>
        }
      </div>
    </section>

    <section class="page event-showcase">
      <div class="showcase-copy">
        <span class="kicker">EVENT MOMENTS</span>
        <h2>Find an experience worth showing up for.</h2>
        <p>From live music and sports to conferences and workshops, discover events and complete your seat, parking and ticket journey in one place.</p>
        <div class="showcase-tags">
          <span>Live music</span><span>Sports</span><span>Conferences</span><span>Workshops</span>
        </div>
        <a class="showcase-link" routerLink="/events">Explore all events <span>→</span></a>
      </div>

      <div class="event-collage" aria-label="EventPark event categories">
        <article class="poster poster-main">
          <img src="/customer-assets/event-music.jpg" alt="Live music event illustration" (error)="staticFallback($event,'/customer-assets/event-music.svg')">
          <div class="poster-overlay"><span>LIVE MUSIC</span><b>Concert nights</b></div>
        </article>
        <div class="poster-stack">
          <article class="poster">
            <img src="/customer-assets/event-sports.jpg" alt="Sports event illustration" (error)="staticFallback($event,'/customer-assets/event-sports.svg')">
            <div class="poster-overlay"><span>SPORTS</span><b>Big match energy</b></div>
          </article>
          <article class="poster">
            <img src="/customer-assets/event-conference.jpg" alt="Conference event illustration" (error)="staticFallback($event,'/customer-assets/event-conference.svg')">
            <div class="poster-overlay"><span>CONFERENCE</span><b>Ideas that connect</b></div>
          </article>
        </div>
      </div>
    </section>

    <section class="page cta">
      <div><span class="kicker light-kicker">READY WHEN YOU ARE</span><h2>Find a seat worth showing up for.</h2><p>Explore events and finish your booking in one simple flow.</p></div>
      <a routerLink="/events">Explore EventPark →</a>
    </section>
  `,
  styles:[`
    .hero{background:linear-gradient(125deg,#f1f8f4 0%,#f7f5ec 58%,#eef8f5 100%);padding:76px 0 54px;overflow:hidden}
    .hero-grid{display:grid;grid-template-columns:.92fr 1.08fr;gap:42px;align-items:center}
    .eyebrow{display:inline-flex;align-items:center;gap:8px;padding:8px 11px;background:#fff;border:1px solid #dce7e4;border-radius:999px;color:#496064;font-size:11px;font-weight:800;box-shadow:0 8px 24px #07363a0c}
    .eyebrow i{width:8px;height:8px;border-radius:50%;background:#f3b21a;box-shadow:0 0 0 5px #f3b21a1f}
    h1{font-size:clamp(49px,6.5vw,82px);line-height:.94;letter-spacing:-4px;margin:24px 0;color:#07363a}
    h1 em{font-style:normal;color:#0b7a69}.copy>p{max-width:610px;color:#5b6f72;font-size:16px;line-height:1.75}
    .hero-actions{display:flex;gap:10px;margin:28px 0 34px}.hero-btn{min-height:48px;padding:0 20px;border-radius:13px;display:inline-flex;align-items:center;gap:12px;font-weight:800;font-size:13px}
    .primary-hero{background:#07363a;color:#fff;box-shadow:0 12px 24px #07363a25}.primary-hero span{color:#f3b21a;font-size:20px}
    .ghost-hero{background:#fff;color:#07363a;border:1px solid #d5e2df}.trust{display:flex;gap:30px;flex-wrap:wrap}.trust div{display:flex;flex-direction:column}.trust b{font-size:18px;color:#0b7a69}.trust span{font-size:10px;color:#748487}
    .hero-visual{min-width:0;border-radius:26px;overflow:hidden;background:#eef7f3;border:1px solid #d5e7df;box-shadow:0 24px 48px #07363a1c}
    .hero-visual img{width:100%;height:auto;display:block;object-fit:cover}
    .feature-strip{margin-top:34px;display:grid;grid-template-columns:repeat(3,1fr);gap:14px}
    .feature-strip article{display:flex;gap:14px;align-items:center;padding:20px;background:#fff;border:1px solid #dce7e4;border-radius:16px;box-shadow:0 12px 34px #07363a0b}
    .feature-strip article>span{width:42px;height:42px;border-radius:12px;background:#e9f7f4;color:#0b7a69;display:grid;place-items:center;font-weight:900;font-size:11px}
    .feature-strip div{display:flex;flex-direction:column;gap:4px}.feature-strip b{font-size:13px}.feature-strip small{color:#728285;line-height:1.4}
    .events-section{padding-top:76px}.section-head{display:flex;justify-content:space-between;align-items:end;gap:25px;margin-bottom:20px}.kicker{font-size:10px;letter-spacing:2px;font-weight:900;color:#0b7a69}
    .section-head h2,.connected-copy h2{font-size:clamp(31px,4vw,45px);letter-spacing:-1.5px;margin:7px 0;color:#07363a}.section-head p,.connected-copy p{color:#718184}.section-head>a{color:#0b7a69;font-weight:800;font-size:12px}
    .event-grid{display:grid;grid-template-columns:repeat(3,1fr);gap:18px}.event-card{background:#fff;border:1px solid #dce7e4;border-radius:19px;overflow:hidden;box-shadow:0 16px 40px #07363a0d;transition:.25s}
    .event-card:hover{transform:translateY(-5px);box-shadow:0 22px 45px #07363a16}.image-wrap{height:190px;position:relative;overflow:hidden;background:#eaf3f1}.image-wrap img{width:100%;height:100%;object-fit:cover;display:block;transition:.35s}
    .event-card:hover img{transform:scale(1.035)}.category,.availability{position:absolute;top:13px;padding:6px 9px;border-radius:999px;font-size:9px;font-weight:900;backdrop-filter:blur(9px)}
    .category{left:13px;background:#ffffffdc;color:#07363a}.availability{right:13px;background:#07363ad9;color:#fff}.availability::first-letter{color:#20b99f}
    .event-content{padding:17px}.event-content h3{margin:0 0 10px;font-size:18px}.event-content p{margin:6px 0;color:#718184;font-size:11px}
    .event-bottom{display:flex;justify-content:space-between;align-items:end;margin-top:18px;padding-top:15px;border-top:1px solid #edf1ef}.event-bottom div{display:flex;flex-direction:column}.event-bottom small{color:#8a989a;font-size:9px}.event-bottom strong{color:#0b7a69;font-size:18px}.event-bottom a{font-size:11px;font-weight:900;color:#07363a}
    .empty-state{grid-column:1/-1;padding:24px;background:#fff;border:1px dashed #cddbd8;border-radius:20px;display:grid;grid-template-columns:.82fr 1.18fr;gap:24px;align-items:center}.empty-copy{padding:10px}.empty-copy>b{display:block;font-size:22px;margin:7px 0;color:#07363a}.empty-copy p{color:#718184;line-height:1.6;margin:0 0 12px}.empty-copy a{color:#0b7a69;font-weight:800;font-size:12px}.preview-events{display:grid;grid-template-columns:repeat(3,1fr);gap:10px}.preview-events article{overflow:hidden;border:1px solid #e1ebe8;border-radius:14px;background:#f8fbfa;position:relative}.preview-events img{width:100%;height:116px;object-fit:cover;display:block}.preview-events span{display:block;padding:9px 10px;font-size:10px;font-weight:900;color:#07363a}
    .event-showcase{margin-top:76px;padding:42px;background:linear-gradient(135deg,#fff 0%,#edf8f5 62%,#fff6df 100%);border:1px solid #dce7e4;border-radius:24px;display:grid;grid-template-columns:.82fr 1.18fr;gap:38px;align-items:center;box-shadow:0 20px 55px #07363a0c;overflow:hidden}
    .showcase-copy h2{font-size:clamp(31px,4vw,45px);letter-spacing:-1.5px;line-height:1.05;margin:8px 0 14px;color:#07363a}.showcase-copy p{color:#718184;line-height:1.75;max-width:510px}.showcase-tags{display:flex;gap:8px;flex-wrap:wrap;margin:20px 0}.showcase-tags span{padding:8px 10px;border-radius:999px;background:#fff;border:1px solid #d8e7e3;color:#496064;font-size:10px;font-weight:800}.showcase-link{display:inline-flex;align-items:center;gap:10px;color:#07363a;font-size:12px;font-weight:900}.showcase-link span{color:#f3b21a;font-size:18px}
    .event-collage{display:grid;grid-template-columns:1.2fr .8fr;gap:12px;min-width:0}.poster-stack{display:grid;grid-template-rows:1fr 1fr;gap:12px;min-width:0}.poster{position:relative;min-height:160px;border-radius:18px;overflow:hidden;background:#eaf3f1;border:1px solid #d6e4e1;box-shadow:0 18px 38px #07363a18}.poster-main{min-height:338px}.poster img{width:100%;height:100%;object-fit:cover;display:block;transition:transform .35s ease}.poster:hover img{transform:scale(1.035)}.poster::after{content:'';position:absolute;inset:0;background:linear-gradient(180deg,transparent 44%,#07363ad6 100%);pointer-events:none}.poster-overlay{position:absolute;left:16px;right:16px;bottom:15px;z-index:1;display:flex;flex-direction:column;gap:3px;color:#fff}.poster-overlay span{font-size:8px;letter-spacing:1.7px;font-weight:900;color:#f6bf37}.poster-overlay b{font-size:16px;letter-spacing:-.3px}.poster-main .poster-overlay b{font-size:22px}
    .cta{margin-top:66px;padding:42px 46px;border-radius:22px;background:linear-gradient(115deg,#0b7a69,#07363a);color:#fff;display:flex;align-items:center;justify-content:space-between;gap:30px;box-shadow:0 22px 55px #07363a20}
    .cta h2{font-size:34px;margin:6px 0}.cta p{color:#c9ded9;margin:0}.light-kicker{color:#f6bf37}.cta>a{padding:13px 18px;border-radius:12px;background:#f3b21a;color:#07363a;font-size:12px;font-weight:900;white-space:nowrap}
    @media(max-width:900px){.hero-grid,.event-showcase{grid-template-columns:1fr}.hero-visual{order:-1;max-width:760px;width:100%;margin:auto}.event-grid{grid-template-columns:1fr 1fr}.empty-state{grid-template-columns:1fr}.event-collage{max-width:680px;width:100%;margin:auto}}
    @media(max-width:700px){.feature-strip{grid-template-columns:1fr}.cta{align-items:flex-start;flex-direction:column}.section-head{align-items:flex-start;flex-direction:column}.event-collage{grid-template-columns:1.15fr .85fr}}
    @media(max-width:540px){.hero{padding-top:42px}.hero-visual{border-radius:18px}h1{letter-spacing:-2px}.hero-actions{flex-direction:column}.hero-btn{justify-content:center}.trust{gap:18px}.event-grid{grid-template-columns:1fr}.preview-events{grid-template-columns:1fr 1fr}.preview-events article:last-child{display:none}.event-showcase{padding:26px 18px}.event-collage{grid-template-columns:1fr}.poster-main{min-height:230px}.poster-stack{grid-template-columns:1fr 1fr;grid-template-rows:1fr}.poster{min-height:145px}.cta{padding:30px 22px}}
  `]
})
export class HomeComponent{
  private api=inject(CustomerApiService);
  readonly items=signal<any[]>([]);
  constructor(){this.api.events().subscribe({next:(r:any)=>this.items.set((r.items??r.data??r??[]).slice(0,6)),error:()=>this.items.set([])})}

  heroFallback(ev:Event){
    const img=ev.target as HTMLImageElement;
    if(!img.src.endsWith('/customer-assets/hero-platform.svg')) img.src='/customer-assets/hero-platform.svg';
  }
  staticFallback(ev:Event,fallback:string){
    const img=ev.target as HTMLImageElement;
    if(!img.src.endsWith(fallback)) img.src=fallback;
  }
  eventFallback(ev:Event,e:any){
    const t=`${e?.categoryName??''} ${e?.name??''}`.toLowerCase();
    let fallback='/customer-assets/event-music.svg';
    if(t.includes('sport')||t.includes('final')||t.includes('match'))fallback='/customer-assets/event-sports.svg';
    else if(t.includes('conference')||t.includes('summit')||t.includes('business')||t.includes('tech'))fallback='/customer-assets/event-conference.svg';
    else if(t.includes('workshop')||t.includes('studio')||t.includes('training'))fallback='/customer-assets/event-workshop.svg';
    this.staticFallback(ev,fallback);
  }
  eventImage(e:any){
    if(e?.imageUrl)return e.imageUrl;
    if(e?.bannerUrl)return e.bannerUrl;
    const t=`${e?.categoryName??''} ${e?.name??''}`.toLowerCase();
    if(t.includes('sport')||t.includes('final')||t.includes('match'))return '/customer-assets/event-sports.jpg';
    if(t.includes('conference')||t.includes('summit')||t.includes('business')||t.includes('tech'))return '/customer-assets/event-conference.jpg';
    if(t.includes('workshop')||t.includes('studio')||t.includes('training'))return '/customer-assets/event-workshop.jpg';
    return '/customer-assets/event-music.jpg';
  }
}
