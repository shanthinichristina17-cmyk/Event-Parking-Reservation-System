import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AdminApiService } from '../admin-api.service';

@Component({
  selector: 'app-bookings-admin',
  imports: [FormsModule],
  template: `
    <main class="pagex bookings-page">
      <div class="head">
        <div><span class="eyebrow">RESERVATION CONTROL</span><h1>Bookings</h1><p>Monitor customers, ticket quantities, parking, payments and booking status.</p></div>
</div>

      <div class="summary-grid">
        <article class="summary card"><span>Bookings shown</span><b>{{rows().length}}</b><small>Current filtered results</small></article>
        <article class="summary card"><span>Tickets sold / held</span><b>{{ticketCount()}}</b><small>Total seats in these bookings</small></article>
        <article class="summary card"><span>Confirmed</span><b>{{countStatus('Confirmed')}}</b><small>Successful reservations</small></article>
        <article class="summary card"><span>Pending / Held</span><b>{{countStatus('Pending')}}</b><small>Awaiting completion</small></article>
      </div>

      <section class="card panel booking-panel">
        <div class="toolbar">
          <div class="searchbox"><span>⌕</span><input class="input" [(ngModel)]="search" (input)="load()" placeholder="Search booking, customer or event"></div>
          <select class="input" [(ngModel)]="status" (change)="load()"><option value="">All booking statuses</option><option>Pending</option><option>Confirmed</option><option>Cancelled</option><option>Expired</option></select>
        </div>

        <div class="table-wrap"><table><thead><tr><th>Booking</th><th>Customer</th><th>Event</th><th>Tickets</th><th>Parking</th><th>Payment</th><th>Status</th><th>Total</th><th>Created</th></tr></thead><tbody>
          @for(b of rows(); track b.bookingId){
            <tr>
              <td><div class="booking-no"><b>{{b.bookingNumber}}</b><small>#{{b.bookingId}}</small></div></td>
              <td><div class="customer-cell"><span>{{initial(b.customerName)}}</span><div><b>{{b.customerName || 'Customer'}}</b><small>{{b.customerEmail || ('ID ' + b.customerId)}}</small></div></div></td>
              <td><b class="event-name">{{b.eventName}}</b></td>
              <td><span class="ticket-count">{{b.seatCount ?? 0}}</span></td>
              <td>@if(b.parkingSlot){<span class="parking-chip">{{b.parkingSlot}}</span>} @else {<span class="muted">No parking</span>}</td>
              <td><span class="payment-chip" [class.paid]="isPaymentDone(b.paymentStatus)" [class.unpaid]="!isPaymentDone(b.paymentStatus)">{{b.paymentStatus || 'Pending'}}</span></td>
              <td><span class="status-chip" [class.confirmed]="same(b.status,'Confirmed')" [class.pending]="same(b.status,'Pending')" [class.cancelled]="same(b.status,'Cancelled')" [class.expired]="same(b.status,'Expired')">{{b.status}}</span></td>
              <td><b>{{money(b.totalAmount ?? b.finalTotal)}}</b></td>
              <td><span class="date">{{dateText(b.createdAt)}}</span></td>
            </tr>
          }
          @empty{<tr><td colspan="9"><div class="empty">No bookings match the current filter.</div></td></tr>}
        </tbody></table></div>
      </section>
    </main>
  `,
  styles: [`
    /* Bookings table width / spacing fix */
    .bookings-page .booking-panel{width:100%!important;box-sizing:border-box!important}
    .bookings-page .table-wrap{
      width:100%!important;
      overflow-x:auto!important;
      box-sizing:border-box!important;
    }
    .bookings-page table{
      width:100%!important;
      min-width:1180px!important;
      table-layout:auto!important;
      border-collapse:separate!important;
      border-spacing:0!important;
    }
    .bookings-page th,
    .bookings-page td{
      padding:12px 14px!important;
      white-space:nowrap!important;
      vertical-align:middle!important;
    }
    .bookings-page th:nth-child(1),
    .bookings-page td:nth-child(1){min-width:170px!important}
    .bookings-page th:nth-child(2),
    .bookings-page td:nth-child(2){min-width:190px!important}
    .bookings-page th:nth-child(3),
    .bookings-page td:nth-child(3){min-width:180px!important}
    .bookings-page th:nth-child(4),
    .bookings-page td:nth-child(4){min-width:80px!important;text-align:center!important}
    .bookings-page th:nth-child(5),
    .bookings-page td:nth-child(5){min-width:105px!important}
    .bookings-page th:nth-child(6),
    .bookings-page td:nth-child(6){min-width:105px!important}
    .bookings-page th:nth-child(7),
    .bookings-page td:nth-child(7){min-width:105px!important}
    .bookings-page th:nth-child(8),
    .bookings-page td:nth-child(8){min-width:105px!important}
    .bookings-page th:nth-child(9),
    .bookings-page td:nth-child(9){min-width:115px!important}

    .eyebrow{font-size:8px;font-weight:950;letter-spacing:1.6px;color:#0f8a78}
    .summary-grid{display:grid;grid-template-columns:repeat(4,1fr);gap:12px;margin-bottom:14px}.summary{padding:16px;display:flex;flex-direction:column;gap:4px;position:relative;overflow:hidden}.summary:before{content:"";position:absolute;left:0;top:0;bottom:0;width:4px;background:#0f8a78}.summary:nth-child(2):before{background:#2563eb}.summary:nth-child(3):before{background:#22a55b}.summary:nth-child(4):before{background:#e7a51a}.summary span,.summary small{font-size:10px;color:#728287}.summary b{font-size:23px;color:#0b2330}
    .booking-panel{padding:0!important;overflow:hidden}.toolbar{display:grid;grid-template-columns:minmax(260px,2fr) minmax(190px,.8fr);gap:10px;padding:16px;border-bottom:1px solid #e8efed;background:#fbfdfc}.searchbox{position:relative}.searchbox>span{position:absolute;left:13px;top:50%;transform:translateY(-50%);color:#7c8d91;font-size:14px}.searchbox input{width:100%;padding-left:35px!important;box-sizing:border-box}.table-wrap{overflow:auto}
    .booking-no,.customer-cell>div{display:flex;flex-direction:column;gap:2px}.booking-no b,.customer-cell b,.event-name{font-size:12px;color:#20383e}.booking-no small,.customer-cell small,.muted,.date{font-size:9px;color:#7b8b90}.customer-cell{display:flex;align-items:center;gap:8px;min-width:150px}.customer-cell>span{width:28px;height:28px;display:grid;place-items:center;border-radius:9px;background:#e8f6f2;color:#0c7767;font-size:9px;font-weight:950}.ticket-count{display:inline-grid;place-items:center;min-width:30px;height:27px;padding:0 7px;border-radius:9px;background:#eaf2ff;color:#1d4ed8;font-weight:950;font-size:11px}.parking-chip{display:inline-flex;padding:6px 8px;border-radius:8px;background:#f1edff;color:#6d28d9;font-size:9px;font-weight:850}
    .status-chip,.payment-chip{display:inline-flex;align-items:center;padding:6px 8px;border-radius:99px;font-size:9px;font-weight:900}.confirmed,.paid{background:#e8f7ed;color:#147a3c}.pending,.unpaid{background:#fff5dc;color:#a86200}.cancelled{background:#ffebeb;color:#c62828}.expired{background:#edf0f2;color:#5d6970}.empty{padding:30px;text-align:center;color:#7a8b90;font-size:10px}
    @media(max-width:1050px){.summary-grid{grid-template-columns:1fr 1fr}}@media(max-width:650px){.summary-grid{grid-template-columns:1fr}.toolbar{grid-template-columns:1fr}}
  `]
})
export class BookingsComponent {
  private api=inject(AdminApiService);readonly rows=signal<any[]>([]);search='';status='';
  constructor(){this.load()}
  load(){this.api.bookings(this.status,this.search).subscribe({next:x=>this.rows.set(Array.isArray(x)?x:[]),error:()=>this.rows.set([])})}
  ticketCount(){return this.rows().reduce((sum,b)=>sum+Number(b.seatCount??0),0)}
  countStatus(status:string){return this.rows().filter(b=>this.same(b.status,status)).length}
  same(a:any,b:string){return String(a||'').toLowerCase()===b.toLowerCase()}
  isPaymentDone(v:any){const s=String(v||'').toLowerCase();return s==='completed'||s==='paid'||s==='success'||s==='successful'}
  initial(name:any){return String(name||'C').trim().charAt(0).toUpperCase()}
  money(v:any){return 'Rs. '+Number(v||0).toLocaleString('en-US')}
  dateText(v:any){if(!v)return '-';const d=new Date(v);if(Number.isNaN(d.getTime()))return String(v);return d.toLocaleDateString('en-GB',{day:'2-digit',month:'short',year:'numeric'})}
}
