import { Component,inject,signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { CustomerApiService } from '../customer-api.service';

@Component({
  selector:'app-events',
  imports:[FormsModule,RouterLink],
  template:`
    <main class="page wrap">
      <div class="hero-head">
        <span>EVENT DISCOVERY</span>
        <h1>Find something worth showing up for.</h1>
        <p>Search live events and continue directly into the seat-and-parking booking flow.</p>
      </div>

      <section class="search-card">
        <div class="searchbox"><span>⌕</span><input [(ngModel)]="search" (keyup.enter)="load()" placeholder="Search event name, venue or category"></div>
        <button (click)="load()">Search events</button>
      </section>

      <div class="result-head"><b>{{items().length}} event(s)</b><span>Connected to live backend inventory</span></div>

      <section class="grid">
        @for(e of items();track e.eventId){
          <article class="item">
            <div class="image">
              <img [src]="eventImage(e)" [alt]="e.name || 'Event image'" (error)="eventFallback($event,e)">
              <span class="live">● LIVE</span>
            </div>
            <div class="content">
              <span class="type">{{e.categoryName || 'Event'}}</span>
              <h3>{{e.name}}</h3>
              <p>◷ {{e.eventDate}} · {{e.startTime}}</p>
              <p>⌖ {{e.venueName || 'Venue'}}</p>
              <div class="bottom"><strong>Rs. {{e.ticketPrice}}</strong><a [routerLink]="['/booking',e.eventId]">Select seats →</a></div>
            </div>
          </article>
        } @empty {
          <div class="empty"><b>No matching events found.</b><p>Try another search or create events from the Admin module.</p></div>
        }
      </section>
    </main>
  `,
  styles:[`
    .wrap{padding:48px 0 20px}.hero-head{max-width:760px}.hero-head>span{font-size:10px;letter-spacing:2px;color:#0b7a69;font-weight:900}.hero-head h1{font-size:clamp(38px,5vw,58px);letter-spacing:-2.4px;line-height:1.02;margin:8px 0;color:#07363a}.hero-head p{color:#6e8083;line-height:1.7}
    .search-card{display:grid;grid-template-columns:1fr auto;gap:10px;margin:28px 0 18px;padding:10px;background:#fff;border:1px solid #dce7e4;border-radius:16px;box-shadow:0 15px 40px #07363a0b}
    .searchbox{display:flex;align-items:center;gap:8px;padding:0 8px}.searchbox span{color:#0b7a69;font-size:20px}.searchbox input{width:100%;border:0;outline:0;min-height:44px;font:inherit;color:#07363a}
    .search-card button{border:0;border-radius:12px;padding:0 18px;background:#07363a;color:#fff;font-weight:900;cursor:pointer}.result-head{display:flex;justify-content:space-between;gap:12px;margin:18px 0;color:#708184;font-size:11px}.result-head b{color:#07363a}
    .grid{display:grid;grid-template-columns:repeat(3,1fr);gap:18px}.item{background:#fff;border:1px solid #dce7e4;border-radius:18px;overflow:hidden;box-shadow:0 14px 38px #07363a0d;transition:.25s}.item:hover{transform:translateY(-4px)}
    .image{height:184px;position:relative;overflow:hidden;background:#edf5f3}.image img{width:100%;height:100%;object-fit:cover;transition:.35s}.item:hover img{transform:scale(1.035)}.live{position:absolute;top:12px;right:12px;background:#07363ad9;color:#fff;padding:6px 9px;border-radius:999px;font-size:9px;font-weight:900}
    .content{padding:17px}.type{font-size:9px;font-weight:900;color:#0b7a69;background:#e8f6f3;padding:5px 8px;border-radius:999px}.content h3{font-size:19px;margin:12px 0}.content p{color:#718184;font-size:11px;margin:7px 0}
    .bottom{display:flex;justify-content:space-between;align-items:center;margin-top:18px;padding-top:15px;border-top:1px solid #edf1ef}.bottom strong{font-size:19px;color:#0b7a69}.bottom a{font-size:11px;font-weight:900;color:#07363a}
    .empty{grid-column:1/-1;padding:40px;text-align:center;background:#fff;border:1px dashed #cbdad6;border-radius:18px}.empty p{color:#728285}
    @media(max-width:850px){.grid{grid-template-columns:1fr 1fr}}@media(max-width:560px){.grid{grid-template-columns:1fr}.search-card{grid-template-columns:1fr}.search-card button{min-height:44px}.result-head{flex-direction:column}}
  `]
})
export class EventsComponent{
  private api=inject(CustomerApiService);
  readonly items=signal<any[]>([]);
  search='';
  constructor(){this.load()}
  load(){this.api.events({search:this.search}).subscribe({next:(r:any)=>this.items.set(r.items??r.data??r??[]),error:()=>this.items.set([])})}

  eventFallback(ev:Event,e:any){
    const img=ev.target as HTMLImageElement;
    const t=`${e?.categoryName??''} ${e?.name??''}`.toLowerCase();
    let fallback='/customer-assets/event-music.svg';
    if(t.includes('sport')||t.includes('final')||t.includes('match'))fallback='/customer-assets/event-sports.svg';
    else if(t.includes('conference')||t.includes('summit')||t.includes('business')||t.includes('tech'))fallback='/customer-assets/event-conference.svg';
    else if(t.includes('workshop')||t.includes('studio')||t.includes('training'))fallback='/customer-assets/event-workshop.svg';
    if(!img.src.endsWith(fallback)) img.src=fallback;
  }
  eventImage(e:any){
    if(e?.imageUrl)return e.imageUrl;if(e?.bannerUrl)return e.bannerUrl;
    const t=`${e?.categoryName??''} ${e?.name??''}`.toLowerCase();
    if(t.includes('sport')||t.includes('final')||t.includes('match'))return '/customer-assets/event-sports.jpg';
    if(t.includes('conference')||t.includes('summit')||t.includes('business')||t.includes('tech'))return '/customer-assets/event-conference.jpg';
    if(t.includes('workshop')||t.includes('studio')||t.includes('training'))return '/customer-assets/event-workshop.jpg';
    return '/customer-assets/event-music.jpg';
  }
}