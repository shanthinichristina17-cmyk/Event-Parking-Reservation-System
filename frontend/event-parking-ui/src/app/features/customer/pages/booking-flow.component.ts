import { Component,inject,OnDestroy,signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute,Router } from '@angular/router';
import { CustomerApiService } from '../customer-api.service';

@Component({
  selector:'app-booking-flow',
  imports:[FormsModule],
  template:`
  <main class="page wrap">
    <div class="flow-head">
      <div><span>PROTECTED BOOKING FLOW</span><h1>{{title()}}</h1><p>Live seat inventory, optional parking and checkout in one connected journey.</p></div>
      @if(step()===3){<div class="hold-badge">⏱ Hold {{minutes()}}:{{seconds()}}</div>}
    </div>

    <div class="steps">
      <div [class.on]="step()===1" [class.done]="step()>1"><b>1</b><span>Seats</span></div><i></i>
      <div [class.on]="step()===2" [class.done]="step()>2"><b>2</b><span>Parking</span></div><i></i>
      <div [class.on]="step()===3" [class.done]="step()>3"><b>3</b><span>Checkout</span></div><i></i>
      <div [class.on]="step()===4"><b>4</b><span>Confirmed</span></div>
    </div>

    @if(step()===1){
      <section class="card booking-panel">
        <div class="panel-top">
          <div><span>LIVE SEAT MAP</span><h2>Choose your seats</h2><p>Select one or more green available seats.</p></div>
          <div class="legend"><span><i class="available-dot"></i>Available</span><span><i class="held-dot"></i>Held</span><span><i class="booked-dot"></i>Booked</span><span><i class="selected-dot"></i>Selected</span></div>
        </div>
        <div class="stage">EVENT STAGE / FIELD</div>
        <div class="seatmap">
          @for(s of seats();track s.seatId){
            <button
              [disabled]="s.status!=='Available'"
              [class.available]="s.status==='Available' && !chosen().has(s.seatId)"
              [class.held]="s.status==='Held'"
              [class.booked]="s.status==='Booked'"
              [class.sel]="chosen().has(s.seatId)"
              (click)="toggle(s.seatId)">
              <b>{{s.seatRow}}{{s.seatNumber}}</b><small>{{s.seatType}}</small>
            </button>
          }
        </div>
        <div class="selection-bar"><span><b>{{chosen().size}}</b> seat(s) selected</span><button class="btn primary" [disabled]="!chosen().size" (click)="hold()">Hold seats & continue →</button></div>
      </section>
    }

    @if(step()===2){
      <section class="card booking-panel">
        <div class="panel-top"><div><span>OPTIONAL PARKING</span><h2>Add parking</h2><p>Choose one available parking slot, or continue without parking.</p></div></div>
        <div class="slots">
          @for(p of parking();track p.slotId){
            <button [disabled]="p.status!=='Available'||p.isDisabled" [class.sel]="parkingId()===p.slotId" (click)="parkingId.set(p.slotId)">
              <span class="parking-icon">P</span><b>{{p.slotNumber}}</b><small>{{p.parkingType}}</small><strong>Rs. {{p.fee}}</strong>
            </button>
          }
        </div>
        <div class="selection-bar"><span>Parking is optional</span><div><button class="btn ghost" (click)="saveParking(null)">Skip parking</button><button class="btn primary" [disabled]="!parkingId()" (click)="saveParking(parkingId())">Continue →</button></div></div>
      </section>
    }

    @if(step()===3&&summary();as b){
      <div class="checkout">
        <section class="card box summary-box">
          <span class="eyebrow">ORDER SUMMARY</span><h2>Protected checkout</h2><p>{{b.eventName}} · {{b.venueName}}</p>
          <div class="line"><span>Tickets</span><b>Rs. {{b.ticketSubtotal}}</b></div>
          <div class="line"><span>Parking</span><b>Rs. {{b.parkingFee}}</b></div>

          <section class="offer-card">
            <img src="/customer-assets/promo-event10.png" alt="EventPark EVENT10 promotional offer">
            <div class="offer-copy">
              <span>🎟 EVENTPARK OFFER</span>
              <b>10% OFF EVENT TICKETS</b>
              <small>Use code: <strong>EVENT10</strong></small>
              <p>Valid for this demo booking experience.</p>
            </div>
            <button class="offer-btn" type="button" (click)="useEvent10()">Apply EVENT10</button>
          </section>

          <div class="promo"><input class="input" [(ngModel)]="promo" placeholder="Promo code"><button class="btn ghost" (click)="applyPromo()">Apply</button></div>
          <div class="line"><span>Discount</span><b>- Rs. {{b.discountAmount}}</b></div>
          <div class="line total"><span>Total</span><b>Rs. {{b.finalTotal}}</b></div>
          <div class="security-note">✓ Your selected seats remain protected while the hold timer is active.</div>
        </section>

        <section class="card box payment-box">
          <span class="eyebrow">PAYMENT SIMULATOR</span><h2>Complete the booking</h2>
          <div class="simulator-note">Demo project payment only — use test values, not real card details.</div>
          <label>Test card number<input class="input" [(ngModel)]="card" placeholder="4111111111111111"></label>
          <div class="two"><label>Expiry<input class="input" [(ngModel)]="expiry" placeholder="12/30"></label><label>CVV<input class="input" [(ngModel)]="cvv" placeholder="123"></label></div>
          <button class="btn pay-btn" [disabled]="remaining()<=0" (click)="pay()">Pay & confirm booking →</button>
        </section>
      </div>
    }

    @if(step()===4&&ticket();as t){
      <div class="confirm">
        <div class="ok">✓</div><span class="eyebrow">BOOKING COMPLETE</span><h1>You're confirmed!</h1><p>Your EventPark ticket is ready.</p>
        <section class="card ticket">
          <div class="tickethead"><div><small>BOOKING NUMBER</small><b>{{t.bookingNumber}}</b></div><span>CONFIRMED</span></div>
          <div class="ticketbody">
            <div>
              <h2>{{t.eventName}}</h2>
              <p>◷ {{t.eventDate}} · {{t.startTime}}</p>
              <p>⌖ {{t.venueName}}</p>
              <strong>Total paid: Rs. {{t.finalTotal||t.totalPaid||'-'}}</strong>
              <div class="ticket-note">Keep this QR on your phone and show it at event entry.</div>
            </div>
            <div class="qr-panel">
              @if(qr()){<img [src]="qr()" alt="Booking QR code">}
              <span>EVENT ENTRY QR</span>
            </div>
          </div>
          <div class="ticket-actions">
            <button class="btn download-btn" type="button" [disabled]="!qr()" (click)="downloadQr()">⬇ Download QR</button>
            <small>PNG format • save it to your phone gallery/files</small>
          </div>
        </section>

        <div class="confirm-actions">
          <button class="btn primary" (click)="downloadQr()" [disabled]="!qr()">Download QR to device</button>
          <button class="btn ghost" (click)="router.navigateByUrl('/my-bookings')">View my bookings →</button>
        </div>
      </div>
    }

    @if(error()){<div class="err">{{error()}}</div>}
  </main>
  `,
  styles:[`
    .wrap{padding:34px 0}.flow-head{display:flex;justify-content:space-between;align-items:center;gap:18px}.flow-head>div>span,.eyebrow{font-size:8px;letter-spacing:1.5px;font-weight:950;color:#0e8f79}.flow-head h1,.confirm>h1{font-size:34px;margin:5px 0;color:#07383a}.flow-head p,.confirm>p{color:#71858a;margin:0}.hold-badge{padding:10px 12px;border-radius:11px;background:#fff5e4;border:1px solid #efc57b;color:#a96900;font-size:10px;font-weight:950}
    .steps{display:flex;align-items:center;margin:22px 0}.steps>div{display:flex;align-items:center;gap:7px;color:#788b8f;font-size:9px;font-weight:900}.steps b{width:29px;height:29px;border-radius:50%;display:grid;place-items:center;background:#e8f0ee;color:#688086}.steps>i{height:2px;flex:1;background:#cfe3df;margin:0 8px}.steps>div.on b,.steps>div.done b{background:#0e8f79;color:#fff}.steps>div.on{color:#0e8f79}.steps>div.done{color:#53736f}
    .booking-panel,.box,.ticket{background:#ffffffeb;border:1px solid #9fd4ca;border-radius:18px;box-shadow:0 16px 38px #07383a0b}.booking-panel{padding:19px}.panel-top{display:flex;justify-content:space-between;gap:16px;align-items:flex-start}.panel-top>div>span{font-size:7px;letter-spacing:1.3px;color:#0e8f79;font-weight:950}.panel-top h2,.box h2{font-size:19px;margin:3px 0;color:#17383d}.panel-top p,.box>p{font-size:9px;color:#71858a;margin:0}.legend{display:flex;gap:8px;flex-wrap:wrap}.legend span{display:flex;align-items:center;gap:4px;font-size:7px;color:#62797e}.legend i{width:9px;height:9px;border-radius:3px}.available-dot{background:#dff6e7;border:1px solid #8fd3a6}.held-dot{background:#fff5d8;border:1px solid #e9c96e}.booked-dot{background:#ffe9e9;border:1px solid #eba9a9}.selected-dot{background:#0e8f79}
    .stage{width:min(520px,70%);margin:22px auto 18px;padding:9px;text-align:center;border-radius:0 0 16px 16px;background:linear-gradient(90deg,#d9e7e4,#eef4f2,#d9e7e4);color:#718489;font-size:8px;font-weight:950;letter-spacing:1.2px}
    .seatmap{display:grid;grid-template-columns:repeat(auto-fill,minmax(64px,1fr));gap:7px}.seatmap button{min-height:50px;border-radius:9px;border:1px solid #d8e5e2;display:flex;flex-direction:column;align-items:center;justify-content:center;cursor:pointer}.seatmap button b{font-size:9px}.seatmap button small{font-size:7px;opacity:.75}.seatmap button.available{background:#e8f8ed;border-color:#a5dcb6;color:#146b38}.seatmap button.held{background:#fff5d8;border-color:#e9c96e;color:#8a5b00}.seatmap button.booked{background:#ffe9e9;border-color:#eba9a9;color:#b42318}.seatmap button.sel{background:#0e8f79!important;border-color:#0e8f79!important;color:#fff!important}.seatmap button:disabled{cursor:not-allowed}
    .selection-bar{margin-top:17px;padding-top:14px;border-top:1px solid #dceae7;display:flex;justify-content:space-between;align-items:center;gap:12px;font-size:9px;color:#6e8589}.selection-bar>span b{font-size:14px;color:#0e8f79}.selection-bar>div{display:flex;gap:7px}
    .btn{min-height:40px;border-radius:9px;padding:0 13px;font-size:9px;font-weight:950;cursor:pointer;border:1px solid transparent}.primary{background:linear-gradient(135deg,#07383a,#0e8f79);color:#fff}.ghost{background:#fff;border-color:#a9d8cf;color:#35555a}.btn:disabled{opacity:.45;cursor:not-allowed}
    .slots{display:grid;grid-template-columns:repeat(5,1fr);gap:9px;margin-top:16px}.slots button{min-height:105px;border:1px solid #b8ddd6;border-radius:12px;background:#f9fcfb;display:flex;flex-direction:column;align-items:center;justify-content:center;gap:3px;cursor:pointer}.slots button.sel{background:#0e8f79;color:#fff;border-color:#0e8f79}.slots button:disabled{opacity:.4;cursor:not-allowed}.parking-icon{width:27px;height:27px;border-radius:8px;background:#e9f7f4;color:#0e8f79;display:grid;place-items:center;font-weight:950}.slots button.sel .parking-icon{background:#ffffff20;color:#fff}.slots b{font-size:9px}.slots small{font-size:7px}.slots strong{font-size:9px;margin-top:3px}
    .offer-card{display:grid;grid-template-columns:86px 1fr auto;gap:12px;align-items:center;padding:12px;border:1px solid #9fd4ca;border-radius:14px;background:linear-gradient(135deg,#f2fbf8,#ffffff);box-shadow:0 10px 24px #07383a0a}.offer-card img{width:86px;height:68px;object-fit:cover;border-radius:10px;border:1px solid #cce8e2}.offer-copy{display:flex;flex-direction:column;gap:2px}.offer-copy>span{font-size:7px;letter-spacing:1.2px;color:#0e8f79;font-weight:950}.offer-copy>b{font-size:13px;color:#07383a}.offer-copy small{font-size:8px;color:#5f777c}.offer-copy small strong{color:#0e8f79}.offer-copy p{font-size:7px;color:#71858a;margin:2px 0 0}.offer-btn{min-height:38px;border:0;border-radius:10px;padding:0 12px;background:linear-gradient(135deg,#0b7a69,#16a085);color:#fff;font-size:8px;font-weight:950;cursor:pointer;white-space:nowrap}.offer-btn:hover{filter:brightness(1.04)}
    .checkout{display:grid;grid-template-columns:1.05fr .95fr;gap:15px}.box{padding:20px;display:flex;flex-direction:column;gap:11px}.line{display:flex;justify-content:space-between;border-bottom:1px dashed #c8ddd8;padding:8px 0;font-size:10px}.promo{display:grid;grid-template-columns:1fr auto;gap:7px}.input{min-height:42px;border:1px solid #b7ddd5;border-radius:9px;padding:0 11px;outline:0}.input:focus{border-color:#0e8f79;box-shadow:0 0 0 3px #0e8f7915}.total{font-size:17px}.total b{color:#0e8f79}.security-note,.simulator-note{padding:10px;border-radius:10px;font-size:8px;line-height:1.5}.security-note{background:#eaf8f0;color:#236f43;border:1px solid #c5e7d0}.simulator-note{background:#fff8e8;color:#805912;border:1px solid #efd49b}.payment-box label{display:flex;flex-direction:column;gap:5px;font-size:8px;font-weight:900;color:#49646a}.two{display:grid;grid-template-columns:1fr 1fr;gap:7px}.pay-btn{background:linear-gradient(135deg,#07383a,#0e8f79,#62a749);color:#fff;border:0}
    .confirm{text-align:center;padding-top:15px}.ok{width:60px;height:60px;border-radius:50%;background:#dcfce7;color:#16a34a;display:grid;place-items:center;margin:0 auto 10px;font-size:28px}.ticket{max-width:700px;margin:18px auto;text-align:left;overflow:hidden}.tickethead{padding:14px 17px;background:linear-gradient(135deg,#07383a,#0e8f79);color:#fff;display:flex;justify-content:space-between;align-items:center}.tickethead>div{display:flex;flex-direction:column}.tickethead small{font-size:7px;color:#bcdad4}.tickethead b{font-size:12px}.tickethead>span{padding:6px 8px;border-radius:99px;background:#ffffff18;font-size:7px;font-weight:950}.ticketbody{padding:18px;display:grid;grid-template-columns:1fr 190px;gap:16px}.ticketbody h2{margin-top:0}.ticketbody p{font-size:9px;color:#70858a}.ticketbody strong{color:#0e8f79}.ticket-note{margin-top:12px;padding:9px;border-radius:9px;background:#eff8f6;color:#567278;font-size:8px}.qr-panel{display:flex;flex-direction:column;align-items:center;gap:6px;padding:10px;border-radius:12px;background:#f8fcfb;border:1px solid #d1e8e3}.qr-panel img{width:170px;height:170px;object-fit:contain}.qr-panel span{font-size:7px;letter-spacing:1px;color:#567278;font-weight:950}.ticket-actions{display:flex;justify-content:space-between;align-items:center;gap:10px;padding:12px 18px;border-top:1px solid #dceae7;background:#fbfefd}.ticket-actions small{font-size:7px;color:#7e9195}.download-btn{background:#0b66dc;color:#fff;border:0}.confirm-actions{display:flex;justify-content:center;gap:8px;flex-wrap:wrap}.err{margin-top:13px;padding:11px 13px;border:1px solid #efb2b2;background:#fff0f0;color:#b42318;border-radius:10px;font-size:9px}
    @media(max-width:800px){.slots{grid-template-columns:repeat(3,1fr)}.checkout{grid-template-columns:1fr}.offer-card{grid-template-columns:70px 1fr}.offer-card img{width:70px;height:58px}.offer-btn{grid-column:1/-1;width:100%}.flow-head{align-items:flex-start;flex-direction:column}}
    @media(max-width:560px){.slots{grid-template-columns:1fr 1fr}.ticketbody{grid-template-columns:1fr}.qr-panel{width:max-content;margin:auto}.selection-bar{align-items:flex-start;flex-direction:column}.steps span{display:none}.ticket-actions{align-items:flex-start;flex-direction:column}}
  `]
})
export class BookingFlowComponent implements OnDestroy{
  private api=inject(CustomerApiService);
  private route=inject(ActivatedRoute);
  readonly router=inject(Router);
  readonly step=signal(1);
  readonly seats=signal<any[]>([]);
  readonly chosen=signal(new Set<number>());
  readonly parking=signal<any[]>([]);
  readonly parkingId=signal<number|null>(null);
  readonly bookingId=signal(0);
  readonly summary=signal<any>(null);
  readonly remaining=signal(0);
  readonly ticket=signal<any>(null);
  readonly qr=signal('');
  readonly error=signal('');
  promo='EVENT10';
  card='4111111111111111';
  expiry='12/30';
  cvv='123';
  private timer?:number;
  eventId=Number(this.route.snapshot.paramMap.get('eventId'));

  constructor(){
    this.api.seats(this.eventId).subscribe({
      next:x=>this.seats.set(x||[]),
      error:e=>this.error.set(e?.error?.message??'Could not load seats. Make sure seats were generated for this event.')
    });
    this.timer=window.setInterval(()=>this.remaining.update(x=>Math.max(0,x-1)),1000)
  }

  title(){return ['Choose your seats','Add optional parking','Protected checkout','Booking confirmed'][this.step()-1]}
  toggle(id:number){const s=new Set(this.chosen());s.has(id)?s.delete(id):s.add(id);this.chosen.set(s)}

  hold(){
    this.error.set('');
    this.api.hold(this.eventId,[...this.chosen()]).subscribe({
      next:b=>{
        this.bookingId.set(b.bookingId);
        this.remaining.set(b.holdSecondsRemaining);
        this.api.parking(this.eventId).subscribe(x=>this.parking.set(x||[]));
        this.step.set(2)
      },
      error:e=>this.error.set(e?.error?.message??'Seat hold failed')
    })
  }

  saveParking(id:number|null){
    this.api.setParking(this.bookingId(),id).subscribe({
      next:b=>{this.summary.set(b);this.remaining.set(b.holdSecondsRemaining);this.step.set(3)},
      error:e=>this.error.set(e?.error?.message??'Parking update failed')
    })
  }

  useEvent10(){this.promo='EVENT10';this.applyPromo()}

  applyPromo(){
    if(!this.promo.trim())return;
    this.api.promo(this.bookingId(),this.promo).subscribe({
      next:b=>{this.summary.set(b);this.error.set('')},
      error:e=>this.error.set(e?.error?.message??'Promo failed')
    })
  }

  pay(){
    this.api.pay(this.bookingId(),{success:true,paymentMethod:'Card',cardNumber:this.card,expiry:this.expiry,cvv:this.cvv}).subscribe({
      next:()=>{
        this.api.ticket(this.bookingId()).subscribe(t=>this.ticket.set(t));
        this.api.qr(this.bookingId()).subscribe(b=>this.qr.set(URL.createObjectURL(b)));
        this.step.set(4)
      },
      error:e=>this.error.set(e?.error?.message??'Payment failed')
    })
  }

  downloadQr(){
    const url=this.qr();
    if(!url){this.error.set('QR ticket is still loading. Please try again.');return}
    const booking=this.ticket()?.bookingNumber || `booking-${this.bookingId()}`;
    const a=document.createElement('a');
    a.href=url;
    a.download=`EventPark-${booking}-QR.png`;
    document.body.appendChild(a);
    a.click();
    a.remove();
  }

  minutes(){return Math.floor(this.remaining()/60).toString().padStart(2,'0')}
  seconds(){return (this.remaining()%60).toString().padStart(2,'0')}
  ngOnDestroy(){if(this.timer)clearInterval(this.timer);if(this.qr())URL.revokeObjectURL(this.qr())}
}
