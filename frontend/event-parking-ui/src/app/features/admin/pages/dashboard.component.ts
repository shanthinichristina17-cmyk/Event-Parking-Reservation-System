import { Component, inject, signal } from '@angular/core';
import { AdminApiService } from '../admin-api.service';

@Component({
  selector: 'app-admin-dashboard',
  template: `
    <main class="pagex admin-dashboard">
      <div class="head">
        <div><span class="eyebrow">OPERATIONS OVERVIEW</span><h1>Dashboard</h1><p>Live platform performance across events, customers, seats, parking and revenue.</p></div>
</div>

      <div class="hero-stats">
        <article class="metric metric-blue"><div class="metric-icon">◎</div><div><span>Total customers</span><b>{{d().totalCustomers ?? 0}}</b><small>{{d().activeCustomers ?? 0}} active accounts</small></div></article>
        <article class="metric metric-violet"><div class="metric-icon">◫</div><div><span>Total events</span><b>{{d().totalEvents ?? 0}}</b><small>{{d().upcomingEvents ?? 0}} upcoming</small></div></article>
        <article class="metric metric-teal"><div class="metric-icon">▣</div><div><span>Total bookings</span><b>{{d().totalBookings ?? 0}}</b><small>{{d().confirmedBookings ?? 0}} confirmed</small></div></article>
        <article class="metric metric-amber"><div class="metric-icon">Rs</div><div><span>Total revenue</span><b>{{money(d().totalRevenue ?? 0)}}</b><small>Completed booking revenue</small></div></article>
      </div>

      <div class="quick-grid">
        <article class="quick card"><span class="dot green"></span><div><small>Available seats</small><b>{{d().availableSeats ?? 0}}</b></div></article>
        <article class="quick card"><span class="dot yellow"></span><div><small>Held seats</small><b>{{d().heldSeats ?? 0}}</b></div></article>
        <article class="quick card"><span class="dot red"></span><div><small>Booked seats</small><b>{{d().bookedSeats ?? 0}}</b></div></article>
        <article class="quick card"><span class="dot blue"></span><div><small>Reserved parking</small><b>{{d().reservedParking ?? 0}}</b></div></article>
      </div>

      <div class="dashboard-grid">
        <section class="card panel revenue-panel">
          <div class="section-title"><div><span>FINANCE</span><h2>Revenue by month</h2></div><small>Last 6 months</small></div>
          <div class="bars">
            @for(x of monthly(); track x.label){
              <div class="bar-col"><div class="bar-value">{{shortMoney(x.netRevenue ?? x.revenue ?? 0)}}</div><div class="bar-track"><i [style.height.%]="height(x.netRevenue ?? x.revenue ?? 0)"></i></div><small>{{x.label}}</small></div>
            }
            @empty{<div class="empty-state">Revenue data will appear after successful payments.</div>}
          </div>
        </section>

        <section class="card panel inventory-panel">
          <div class="section-title"><div><span>LIVE INVENTORY</span><h2>Seat availability</h2></div></div>
          <div class="inventory-row"><div><span class="legend green"></span><b>Available</b><em>{{d().availableSeats ?? 0}}</em></div><div class="progress"><i class="p-green" [style.width.%]="seatPercent(d().availableSeats ?? 0)"></i></div></div>
          <div class="inventory-row"><div><span class="legend yellow"></span><b>Held</b><em>{{d().heldSeats ?? 0}}</em></div><div class="progress"><i class="p-yellow" [style.width.%]="seatPercent(d().heldSeats ?? 0)"></i></div></div>
          <div class="inventory-row"><div><span class="legend red"></span><b>Booked</b><em>{{d().bookedSeats ?? 0}}</em></div><div class="progress"><i class="p-red" [style.width.%]="seatPercent(d().bookedSeats ?? 0)"></i></div></div>
          <div class="inventory-summary"><div><small>Pending bookings</small><b>{{d().pendingBookings ?? 0}}</b></div><div><small>Cancelled bookings</small><b>{{d().cancelledBookings ?? 0}}</b></div></div>
        </section>

        <section class="card panel parking-panel">
          <div class="section-title"><div><span>PARKING</span><h2>Parking utilization</h2></div></div>
          <div class="parking-numbers"><div><small>Available</small><b>{{d().availableParking ?? 0}}</b></div><div><small>Held</small><b>{{d().heldParking ?? 0}}</b></div><div><small>Reserved</small><b>{{d().reservedParking ?? 0}}</b></div></div>
          <div class="parking-track"><i class="park-reserved" [style.width.%]="parkingPercent(d().reservedParking ?? 0)"></i><i class="park-held" [style.width.%]="parkingPercent(d().heldParking ?? 0)"></i></div>
          <p>Parking occupancy updates from customer reservations and confirmed bookings.</p>
        </section>

        <section class="card panel alerts-panel">
          <div class="section-title"><div><span>SYSTEM</span><h2>Operational alerts</h2></div><span class="live-badge">LIVE</span></div>
          @for(a of alerts(); track $index){<div class="alert-row"><span class="alert-mark">!</span><div><b>{{a.severity || 'Info'}}</b><p>{{a.message || a.title || a.type}}</p></div></div>}
          @empty{<div class="all-clear"><span>✓</span><div><b>All clear</b><p>No urgent operational alerts.</p></div></div>}
        </section>
      </div>
    </main>
  `,
  styles: [`
    .eyebrow{font-size:8px;font-weight:950;letter-spacing:1.7px;color:#0f8a78}
    .hero-stats{display:grid;grid-template-columns:repeat(4,1fr);gap:14px;margin-bottom:14px}.metric{min-height:132px;border-radius:18px;padding:18px;display:flex;gap:14px;align-items:flex-start;color:#fff;box-shadow:0 16px 35px rgba(16,42,47,.09)}.metric-icon{width:40px;height:40px;border-radius:12px;background:rgba(255,255,255,.17);display:grid;place-items:center;font-size:13px;font-weight:950}.metric>div:last-child{display:flex;flex-direction:column;gap:5px;min-width:0}.metric span{font-size:9px;text-transform:uppercase;letter-spacing:.7px;opacity:.78}.metric b{font-size:24px;line-height:1.15;letter-spacing:-.7px}.metric small{font-size:9px;opacity:.72}.metric-blue{background:linear-gradient(135deg,#1d4ed8,#2563eb)}.metric-violet{background:linear-gradient(135deg,#6d28d9,#8b5cf6)}.metric-teal{background:linear-gradient(135deg,#0b665c,#0f9b83)}.metric-amber{background:linear-gradient(135deg,#b45309,#e59b17)}
    .quick-grid{display:grid;grid-template-columns:repeat(4,1fr);gap:12px;margin-bottom:14px}.quick{padding:14px 16px;display:flex;align-items:center;gap:11px}.quick .dot{width:10px;height:10px;border-radius:50%}.quick div{display:flex;flex-direction:column}.quick small{font-size:9px;color:#6b7c82}.quick b{font-size:18px;color:#0b2330}.green{background:#22a55b}.yellow{background:#e7a51a}.red{background:#dc3d3d}.blue{background:#3b82f6}
    .dashboard-grid{display:grid;grid-template-columns:1.25fr .85fr;gap:14px}.section-title{display:flex;align-items:flex-start;justify-content:space-between;gap:10px;margin-bottom:16px}.section-title span{font-size:8px;color:#0f8a78;font-weight:950;letter-spacing:1.4px}.section-title h2{margin:3px 0 0;font-size:16px}.section-title>small{font-size:9px;color:#849398}
    .bars{height:245px;display:flex;align-items:flex-end;gap:11px;padding-top:10px}.bar-col{height:100%;flex:1;display:flex;flex-direction:column;justify-content:flex-end;align-items:center;gap:6px;min-width:0}.bar-track{height:180px;width:72%;background:#eef4f2;border-radius:8px;display:flex;align-items:flex-end;overflow:hidden}.bar-track i{width:100%;min-height:5px;background:linear-gradient(180deg,#17a98f,#0b665c);border-radius:8px 8px 3px 3px}.bar-col small{font-size:8px;color:#6b7c82;white-space:nowrap}.bar-value{font-size:8px;font-weight:850;color:#294147}.empty-state{margin:auto;color:#7f8f92;font-size:10px;text-align:center}
    .inventory-row{margin-bottom:16px}.inventory-row>div:first-child{display:flex;align-items:center;gap:8px;margin-bottom:6px}.inventory-row b{font-size:10px;color:#2b4146}.inventory-row em{margin-left:auto;font-size:10px;font-style:normal;font-weight:900}.legend{width:8px;height:8px;border-radius:50%}.progress{height:8px;border-radius:99px;background:#edf3f1;overflow:hidden}.progress i{display:block;height:100%;border-radius:99px}.p-green{background:#22a55b}.p-yellow{background:#e7a51a}.p-red{background:#dc3d3d}.inventory-summary{display:grid;grid-template-columns:1fr 1fr;gap:10px;padding-top:8px}.inventory-summary>div{padding:12px;border-radius:12px;background:#f6f9f8;display:flex;flex-direction:column}.inventory-summary small{font-size:8px;color:#708084}.inventory-summary b{font-size:17px;color:#0b2330}
    .parking-numbers{display:grid;grid-template-columns:repeat(3,1fr);gap:8px;margin-bottom:14px}.parking-numbers>div{padding:13px;border-radius:12px;background:#f6f9f8;display:flex;flex-direction:column}.parking-numbers small{font-size:8px;color:#718186}.parking-numbers b{font-size:18px;color:#0b2330}.parking-track{height:11px;border-radius:99px;background:#dff1e9;display:flex;overflow:hidden;margin-bottom:12px}.parking-track i{height:100%;display:block}.park-reserved{background:#ef4444}.park-held{background:#e7a51a}.parking-panel p{font-size:9px;line-height:1.6;color:#738388;margin:0}
    .live-badge{padding:5px 7px;border-radius:7px;background:#e7f7f1;color:#0d7e69!important;letter-spacing:.7px!important}.alert-row,.all-clear{display:flex;gap:10px;padding:11px 0;border-bottom:1px solid #edf2f1}.alert-row:last-child{border-bottom:0}.alert-mark,.all-clear>span{width:28px;height:28px;flex:0 0 28px;border-radius:9px;display:grid;place-items:center;font-size:10px;font-weight:950}.alert-mark{background:#fff4e6;color:#c56a00}.all-clear>span{background:#eaf8ef;color:#15803d}.alert-row b,.all-clear b{font-size:10px;color:#294147}.alert-row p,.all-clear p{font-size:9px;color:#74858a;margin:3px 0 0;line-height:1.45}
    @media(max-width:1100px){.hero-stats{grid-template-columns:1fr 1fr}.quick-grid{grid-template-columns:1fr 1fr}}@media(max-width:850px){.dashboard-grid{grid-template-columns:1fr}}@media(max-width:560px){.hero-stats,.quick-grid{grid-template-columns:1fr}.metric{min-height:100px}}
  `]
})
export class DashboardComponent {
  private api=inject(AdminApiService);readonly d=signal<any>({});readonly monthly=signal<any[]>([]);readonly alerts=signal<any[]>([]);
  constructor(){this.load()}
  load(){this.api.dashboard().subscribe(x=>this.d.set(x));this.api.monthly().subscribe(x=>this.monthly.set(x));this.api.alerts().subscribe({next:x=>this.alerts.set(x),error:()=>this.alerts.set([])})}
  money(v:any){return 'Rs. '+Number(v||0).toLocaleString('en-US')}
  shortMoney(v:any){const n=Number(v||0);if(n>=1000000)return 'Rs.'+(n/1000000).toFixed(1)+'M';if(n>=1000)return 'Rs.'+(n/1000).toFixed(0)+'K';return 'Rs.'+n.toFixed(0)}
  height(v:number){const values=this.monthly().map(x=>Number(x.netRevenue??x.revenue??0));const max=Math.max(...values,1);return Math.max(5,Number(v||0)/max*100)}
  seatPercent(v:number){const d=this.d();const total=Number(d.availableSeats||0)+Number(d.heldSeats||0)+Number(d.bookedSeats||0);return total?Math.max(2,Number(v||0)/total*100):0}
  parkingPercent(v:number){const d=this.d();const total=Number(d.availableParking||0)+Number(d.heldParking||0)+Number(d.reservedParking||0);return total?Number(v||0)/total*100:0}
}
