import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AdminApiService } from '../admin-api.service';

@Component({
  selector:'app-reports-admin',
  imports:[FormsModule],
  template:`
  <main class="pagex reports-page">
    <div class="head">
      <div><span class="eyebrow">BUSINESS INTELLIGENCE</span><h1>Reports & Analytics</h1><p>Track bookings, revenue, ticket sales and parking performance with live backend data.</p></div>
      <div class="export-actions">
        <button class="export-btn" (click)="download('events.csv')">Events CSV</button>
        <button class="export-btn" (click)="download('bookings.csv')">Bookings CSV</button>
        <button class="export-btn strong" (click)="download('payments.csv')">Payments CSV</button>
      </div>
    </div>

    <section class="card report-filter">
      <div><label>From date</label><input class="input" [(ngModel)]="from" type="date"></div>
      <div><label>To date</label><input class="input" [(ngModel)]="to" type="date"></div>
      <button class="btn primary" (click)="load()">Apply report filter</button>
      <button class="btn reset" (click)="clear()">Reset</button>
    </section>

    @if(s();as x){
      <div class="kpis">
        <article class="kpi blue"><small>Total bookings</small><b>{{x.totalBookings||0}}</b><span>All reservations</span></article>
        <article class="kpi green"><small>Confirmed</small><b>{{x.confirmedBookings||0}}</b><span>Successful bookings</span></article>
        <article class="kpi amber"><small>Pending</small><b>{{x.pendingBookings||0}}</b><span>Awaiting completion</span></article>
        <article class="kpi red"><small>Cancelled</small><b>{{x.cancelledBookings||0}}</b><span>Cancelled reservations</span></article>
        <article class="kpi teal"><small>Revenue</small><b>{{money(x.revenue||0)}}</b><span>Confirmed revenue</span></article>
        <article class="kpi violet"><small>Avg. booking</small><b>{{money(x.averageBookingValue||0)}}</b><span>Average booking value</span></article>
        <article class="kpi cyan"><small>Seats sold</small><b>{{x.seatsSold||0}}</b><span>Ticket / seat volume</span></article>
        <article class="kpi navy"><small>Parking reservations</small><b>{{x.parkingReservations||0}}</b><span>Reserved parking slots</span></article>
      </div>
    }

    <div class="report-grid">
      <section class="card panel revenue-section">
        <div class="section-head"><div><span>EVENT PERFORMANCE</span><h2>Revenue by event</h2></div><small>{{rev().length}} events</small></div>
        <div class="revenue-list">
          @for(r of rev();track r.eventId??$index){
            <div class="revenue-row">
              <div class="event-number">{{($index+1).toString().padStart(2,'0')}}</div>
              <div class="event-info"><b>{{r.eventName||r.name}}</b><small>{{r.confirmedBookings??r.bookingCount??0}} confirmed bookings</small></div>
              <div class="event-bar"><i [style.width.%]="revenuePercent(r.revenue??0)"></i></div>
              <strong>{{money(r.revenue??0)}}</strong>
            </div>
          }
          @empty{<div class="empty-state">Revenue data will appear after confirmed payments.</div>}
        </div>
      </section>

      <section class="card panel status-section">
        <div class="section-head"><div><span>BOOKING HEALTH</span><h2>Status breakdown</h2></div></div>
        @for(r of statuses();track r.status??$index){
          <div class="status-row">
            <div><span class="status-dot" [class]="statusClass(r.status)"></span><b>{{r.status}}</b><strong>{{r.count||0}}</strong></div>
            <div class="status-track"><i [class]="statusClass(r.status)" [style.width.%]="statusPercent(r.count||0)"></i></div>
          </div>
        }
        @empty{<div class="empty-state">No booking status data available.</div>}
      </section>
    </div>
  </main>
  `,
  styles:[`
    .eyebrow{font-size:7px;font-weight:950;letter-spacing:1.5px;color:#11907b}.export-actions{display:flex;gap:6px;flex-wrap:wrap;justify-content:flex-end}.export-btn{border:1.4px solid #91cfc4;background:#ffffffdc;color:#245057;padding:10px 12px;border-radius:9px;font-size:10px;font-weight:900;cursor:pointer}.export-btn.strong{background:#0b6a61;color:white;border-color:#0b6a61}
    .report-filter{padding:14px 16px;margin-bottom:13px;display:grid;grid-template-columns:1fr 1fr auto auto;gap:9px;align-items:end}.report-filter>div{display:flex;flex-direction:column;gap:5px}.report-filter label{font-size:10px;font-weight:850;color:#526b70}.reset{background:#fff!important;color:#556d72!important;border:1.4px solid #a7d6cd!important}
    .kpis{display:grid;grid-template-columns:repeat(4,1fr);gap:10px;margin-bottom:13px}.kpi{position:relative;overflow:hidden;min-height:102px;padding:14px 15px;border-radius:14px;color:white;display:flex;flex-direction:column;box-shadow:0 11px 25px #174b4b14}.kpi:after{content:"";position:absolute;width:70px;height:70px;border-radius:50%;right:-20px;top:-22px;background:#ffffff14}.kpi small{font-size:9px;text-transform:uppercase;letter-spacing:.8px;opacity:.8}.kpi b{font-size:19px;margin:4px 0}.kpi span{font-size:9px;opacity:.72}.blue{background:linear-gradient(135deg,#1e4fc2,#3877e8)}.green{background:linear-gradient(135deg,#137b45,#22a65e)}.amber{background:linear-gradient(135deg,#a86100,#de9918)}.red{background:linear-gradient(135deg,#b12b2b,#dd4b4b)}.teal{background:linear-gradient(135deg,#09665e,#16a08a)}.violet{background:linear-gradient(135deg,#6330b6,#8855d9)}.cyan{background:linear-gradient(135deg,#0d7289,#1f9eba)}.navy{background:linear-gradient(135deg,#143642,#1f5964)}
    .report-grid{display:grid;grid-template-columns:1.3fr .7fr;gap:13px}.section-head{display:flex;justify-content:space-between;align-items:flex-start;margin-bottom:12px}.section-head span{font-size:9px;color:#11907b;font-weight:950;letter-spacing:1.2px}.section-head h2{margin:2px 0 0!important;font-size:15px!important}.section-head small{font-size:9px;color:#778b90}
    .revenue-row{display:grid;grid-template-columns:28px minmax(150px,1fr) minmax(120px,.8fr) auto;align-items:center;gap:9px;padding:10px 0;border-bottom:1px solid #dcece8}.event-number{width:25px;height:25px;border-radius:8px;background:#e8f7f3;color:#0a7969;display:grid;place-items:center;font-size:7px;font-weight:950}.event-info{display:flex;flex-direction:column}.event-info b{font-size:11px}.event-info small{font-size:9px;color:#71868b}.event-bar{height:7px;border-radius:99px;background:#e9f2f0;overflow:hidden}.event-bar i{display:block;height:100%;background:linear-gradient(90deg,#0c7066,#23b397);border-radius:99px}.revenue-row>strong{font-size:10px;color:#174248;white-space:nowrap}
    .status-row{margin-bottom:13px}.status-row>div:first-child{display:flex;align-items:center;gap:7px;margin-bottom:5px}.status-row b{font-size:11px}.status-row strong{margin-left:auto;font-size:11px}.status-dot{width:8px;height:8px;border-radius:50%}.status-track{height:8px;border-radius:99px;background:#e9f2f0;overflow:hidden}.status-track i{display:block;height:100%;border-radius:99px}.confirmed{background:#22a55b!important}.pending{background:#e6a31a!important}.cancelled{background:#dc4242!important}.expired{background:#78858a!important}.other{background:#2f7adf!important}
    .empty-state{padding:28px;text-align:center;color:#72868b;font-size:9px}
    @media(max-width:1050px){.kpis{grid-template-columns:1fr 1fr}.report-grid{grid-template-columns:1fr}}
    @media(max-width:650px){.report-filter{grid-template-columns:1fr}.kpis{grid-template-columns:1fr 1fr}.revenue-row{grid-template-columns:28px 1fr auto}.event-bar{display:none}}
  `]
})
export class ReportsComponent{
  private api=inject(AdminApiService);
  readonly s=signal<any>({});
  readonly rev=signal<any[]>([]);
  readonly statuses=signal<any[]>([]);
  from='';to='';
  constructor(){this.load()}
  load(){
    this.api.summary(this.from,this.to).subscribe(x=>this.s.set(x));
    this.api.revenue(this.from,this.to).subscribe(x=>this.rev.set(x));
    this.api.statusReport(this.from,this.to).subscribe(x=>this.statuses.set(x));
  }
  clear(){this.from='';this.to='';this.load()}
  money(v:any){return 'Rs. '+Number(v||0).toLocaleString('en-US',{maximumFractionDigits:2})}
  revenuePercent(v:any){const m=Math.max(...this.rev().map(x=>Number(x.revenue||0)),1);return Math.max(2,Number(v||0)/m*100)}
  statusPercent(v:any){const m=Math.max(...this.statuses().map(x=>Number(x.count||0)),1);return Math.max(3,Number(v||0)/m*100)}
  statusClass(s:any){const x=String(s||'').toLowerCase();return ['confirmed','pending','cancelled','expired'].includes(x)?x:'other'}
  download(n:string){this.api.export(n).subscribe(b=>{const u=URL.createObjectURL(b);const a=document.createElement('a');a.href=u;a.download=n;a.click();URL.revokeObjectURL(u)})}
}
