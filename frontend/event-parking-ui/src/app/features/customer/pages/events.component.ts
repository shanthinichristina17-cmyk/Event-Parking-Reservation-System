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
        <h1>Find your next EventPark experience.</h1>
        <p>Search the live backend, filter by category and move directly into seat and parking selection.</p>
      </div>

      <section class="search-card">
        <div class="searchbox"><span>⌕</span><input [(ngModel)]="search" (keyup.enter)="load()" placeholder="Search event, venue or category"></div>
        <button (click)="load()">Search events</button>
      </section>

      <div class="category-filter">
        @for(c of categories;track c){
          <button [class.active]="category()===c" (click)="category.set(c)">{{c}}</button>
        }
      </div>

      <div class="result-head">
        <div><b>{{filtered().length}} event(s)</b><span>Live inventory from EventPark API</span></div>
        <small>Available events update when Admin changes inventory.</small>
      </div>

      <section class="grid">
        @for(e of filtered();track e.eventId){
          <article class="item">
            <div class="image">
              <img [src]="eventImage(e)" [alt]="e.name || 'Event image'">
              <span class="live">● LIVE</span>
              <span class="type">{{e.categoryName || 'Event'}}</span>
            </div>
            <div class="content">
              <h3>{{e.name}}</h3>
              <div class="meta">
                <span><i>◷</i>{{e.eventDate}} · {{e.startTime}}</span>
                <span><i>⌖</i>{{e.venueName || 'Venue'}}</span>
              </div>
              <div class="capacity">
                <span>Event capacity</span><b>{{e.capacity || 'Live'}}</b>
              </div>
              <div class="bottom">
                <div><small>Ticket price</small><strong>Rs. {{e.ticketPrice}}</strong></div>
                <a [routerLink]="['/booking',e.eventId]">Select seats →</a>
              </div>
            </div>
          </article>
        } @empty {
          <div class="empty"><b>No matching events found.</b><p>Try another category or search term.</p><button (click)="reset()">Reset filters</button></div>
        }
      </section>
    </main>
  `,
  styles:[`
    .wrap{padding:48px 0 24px}.hero-head{max-width:780px}.hero-head>span{font-size:11px;letter-spacing:2px;color:#0e8f79;font-weight:950}.hero-head h1{font-size:clamp(39px,5vw,58px);letter-spacing:-2.3px;line-height:1.02;margin:8px 0;color:#07383a}.hero-head p{color:#657d82;line-height:1.7;font-size:16px}
    .search-card{display:grid;grid-template-columns:1fr auto;gap:9px;margin:24px 0 12px;padding:9px;background:#ffffffdf;border:1px solid #9fd4ca;border-radius:15px;box-shadow:0 14px 35px #07383a0b}.searchbox{display:flex;align-items:center;gap:8px;padding:0 8px}.searchbox span{color:#0e8f79;font-size:18px}.searchbox input{width:100%;border:0;outline:0;min-height:43px;background:transparent;font:inherit;color:#07383a}.search-card button{border:0;border-radius:11px;padding:0 17px;background:linear-gradient(135deg,#07383a,#0e8f79);color:#fff;font-weight:900;cursor:pointer}
    .category-filter{display:flex;gap:7px;flex-wrap:wrap;margin:13px 0 20px}.category-filter button{padding:8px 11px;border:1px solid #a6d8cf;background:#ffffffd9;border-radius:99px;color:#456167;font-size:11px;font-weight:900;cursor:pointer}.category-filter button.active{background:#0e8f79;color:#fff;border-color:#0e8f79}
    .result-head{display:flex;justify-content:space-between;gap:18px;margin-bottom:14px;color:#71858a;font-size:12px}.result-head>div{display:flex;gap:10px;align-items:center}.result-head b{color:#07383a;font-size:14px}.result-head small{font-size:11px}
    .grid{display:grid;grid-template-columns:repeat(3,1fr);gap:16px}.item{background:#ffffffeb;border:1px solid #9fd4ca;border-radius:18px;overflow:hidden;box-shadow:0 14px 38px #07383a0d;transition:.22s}.item:hover{transform:translateY(-4px);box-shadow:0 20px 42px #07383a16}.image{height:205px;position:relative;overflow:hidden;background:#edf5f3}.image img{width:100%;height:100%;object-fit:cover;transition:.32s}.item:hover img{transform:scale(1.035)}.live,.type{position:absolute;top:12px;padding:6px 9px;border-radius:999px;font-size:8px;font-weight:950}.live{right:12px;background:#07383add;color:#fff}.type{left:12px;background:#ffffffe8;color:#07383a}
    .content{padding:17px}.content h3{font-size:21px;margin:0 0 12px;color:#17343a}.meta{display:flex;flex-direction:column;gap:7px;color:#6e8589;font-size:12px}.meta span{display:flex;align-items:center;gap:6px}.meta i{width:22px;height:22px;border-radius:7px;background:#eaf7f4;color:#0e8f79;display:grid;place-items:center;font-style:normal}
    .capacity{display:flex;justify-content:space-between;padding:10px 11px;background:#f2faf8;border:1px solid #d2ebe5;border-radius:10px;margin-top:12px;font-size:11px;color:#71858a}.capacity b{color:#16444a}
    .bottom{display:flex;justify-content:space-between;align-items:end;margin-top:14px;padding-top:13px;border-top:1px solid #dceae7}.bottom>div{display:flex;flex-direction:column}.bottom small{font-size:10px;color:#87979a}.bottom strong{font-size:21px;color:#0e8f79}.bottom a{padding:10px 12px;border-radius:9px;background:#07383a;color:#fff;text-decoration:none;font-size:11px;font-weight:950}
    .empty{grid-column:1/-1;padding:42px;text-align:center;background:#ffffffdc;border:1px dashed #9fcfc6;border-radius:18px}.empty p{color:#71858a}.empty button{border:0;border-radius:9px;background:#0e8f79;color:#fff;padding:9px 13px;font-weight:900;cursor:pointer}
    @media(max-width:870px){.grid{grid-template-columns:1fr 1fr}}@media(max-width:570px){.grid{grid-template-columns:1fr}.search-card{grid-template-columns:1fr}.search-card button{min-height:43px}.result-head{flex-direction:column}.result-head>div{flex-wrap:wrap}}
  `]
})
export class EventsComponent{
  private api=inject(CustomerApiService);
  readonly items=signal<any[]>([]);
  readonly category=signal('All');
  search='';
  readonly categories=['All','Concert','Conference','Seminar','Sports','Webinar','Workshop'];

  constructor(){this.load()}

  load(){
    this.api.events({search:this.search,page:1,pageSize:100}).subscribe({
      next:(r:any)=>this.items.set(r?.items??r?.data??r??[]),
      error:()=>this.items.set([])
    })
  }

  filtered(){
    const c=this.category().toLowerCase();
    if(c==='all')return this.items();
    return this.items().filter(e=>String(e?.categoryName||'').toLowerCase()===c);
  }

  reset(){this.search='';this.category.set('All');this.load()}

  eventImage(e:any){
    const t=`${e?.categoryName??''} ${e?.name??''}`.toLowerCase();
    if(t.includes('conference')||t.includes('summit'))return '/customer-assets/categories/conference.jpg';
    if(t.includes('seminar'))return '/customer-assets/categories/seminar.jpg';
    if(t.includes('sport')||t.includes('match')||t.includes('final'))return '/customer-assets/categories/sports.jpg';
    if(t.includes('webinar')||t.includes('online'))return '/customer-assets/categories/webinar.jpg';
    if(t.includes('workshop')||t.includes('training')||t.includes('bootcamp'))return '/customer-assets/categories/workshop.jpg';
    return '/customer-assets/categories/concert.jpg';
  }
}
