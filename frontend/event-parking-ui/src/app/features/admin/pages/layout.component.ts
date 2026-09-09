import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { concatMap, from, toArray } from 'rxjs';
import { AdminApiService } from '../admin-api.service';

@Component({
  selector: 'app-layout-builder',
  imports: [FormsModule],
  template: `
    <main class="pagex layout-page">
      <div class="head">
        <div><span class="eyebrow">EVENT INVENTORY</span><h1>Seats & Parking</h1><p>Generate event layouts, configure prices and monitor live availability.</p></div>
        <div class="legend"><span><i class="lg available"></i>Available</span><span><i class="lg held"></i>Held</span><span><i class="lg booked"></i>Booked</span></div>
      </div>

      <section class="card event-picker">
        <div><label>Selected event</label><select class="input" [(ngModel)]="eventId" (change)="load()"><option [ngValue]="0">Choose an event</option>@for(e of events(); track e.eventId){<option [ngValue]="e.eventId">{{e.name}}</option>}</select></div>
        <div class="selected-info"><small>Live layout</small><b>{{eventId ? (seatRows().length + ' seats · ' + parkRows().length + ' parking slots') : 'Select an event to begin'}}</b></div>
      </section>

      <div class="layout-columns">
        <section class="card config-card">
          <div class="config-head"><div class="config-icon seat-icon">S</div><div><span>SEAT CONFIGURATION</span><h2>Seat layout & pricing</h2></div></div>
          <div class="form-grid">
            <label>Rows<input class="input" [(ngModel)]="rows" type="number" min="1"></label>
            <label>Columns<input class="input" [(ngModel)]="cols" type="number" min="1"></label>
            <label>Regular price<input class="input" [(ngModel)]="regularPrice" type="number" min="0"></label>
            <label>Premium price<input class="input" [(ngModel)]="premiumPrice" type="number" min="0"></label>
            <label>VIP price<input class="input" [(ngModel)]="vipPrice" type="number" min="0"></label>
          </div>
          <div class="action-row"><button class="btn primary" (click)="makeSeats()">Generate seats</button><button class="btn outline danger-btn" (click)="resetSeats()">Reset available seats</button></div>
        </section>

        <section class="card config-card">
          <div class="config-head"><div class="config-icon park-icon">P</div><div><span>PARKING CONFIGURATION</span><h2>Add parking zone</h2></div></div>
          <div class="form-grid parking-form">
            <label>Zone<input class="input" [(ngModel)]="zone" placeholder="Example: B"></label>
            <label>Parking type<select class="input" [(ngModel)]="parkingType"><option value="Standard">Standard</option><option value="VIP">VIP</option><option value="Normal">Normal</option></select></label>
            <label>Slot count<input class="input" [(ngModel)]="slots" type="number" min="1"></label>
            <label>Fee<input class="input" [(ngModel)]="fee" type="number" min="0"></label>
          </div>
          <div class="action-row"><button class="btn primary" (click)="makeOrAddParking()">{{parkRows().length > 0 ? 'Add parking zone' : 'Generate parking'}}</button></div>
        </section>
      </div>

      <section class="inventory-strip">
        <article class="inventory-card available-card"><span>Available seats</span><b>{{availableSeats()}}</b><small>Ready to book</small></article>
        <article class="inventory-card held-card"><span>Held seats</span><b>{{heldSeats()}}</b><small>Temporary hold</small></article>
        <article class="inventory-card booked-card"><span>Booked seats</span><b>{{bookedSeats()}}</b><small>Confirmed / sold</small></article>
        <article class="inventory-card parking-card"><span>Parking slots</span><b>{{parkRows().length}}</b><small>{{availableParking()}} available</small></article>
      </section>

      <section class="card map-card">
        <div class="map-head"><div><span>SEAT MAP</span><h2>Live seat availability</h2></div><small>{{seatRows().length}} total seats</small></div>
        @if(!eventId){<div class="empty-state">Select an event to view its seat layout.</div>}
        @else if(seatRows().length === 0){<div class="empty-state">No seats generated for this event yet.</div>}
        @else{
          <div class="stage">EVENT STAGE / FRONT</div>
          <div class="seatmap">
            @for(s of seatRows(); track s.seatId){
              <div class="seat" [class.available]="statusIs(s.status,'Available')" [class.held]="statusIs(s.status,'Held')" [class.booked]="statusIs(s.status,'Booked')">
                <b>{{s.seatRow}}{{s.seatNumber}}</b><small>{{s.seatType}}</small><em>Rs. {{s.price}}</em>
              </div>
            }
          </div>
        }
      </section>

      <section class="card map-card">
        <div class="map-head"><div><span>PARKING MAP</span><h2>Parking inventory</h2></div><small>{{parkRows().length}} total slots</small></div>
        @if(!eventId){<div class="empty-state">Select an event to view parking.</div>}
        @else if(parkRows().length === 0){<div class="empty-state">No parking layout has been generated.</div>}
        @else{
          <div class="parkmap">
            @for(p of parkRows(); track p.slotId){
              <div class="parking-slot" [class.available]="statusIs(p.status,'Available')" [class.held]="statusIs(p.status,'Held')" [class.booked]="statusIs(p.status,'Booked') || statusIs(p.status,'Reserved')" [class.disabled]="p.isDisabled || statusIs(p.status,'Disabled')">
                <b>{{p.slotNumber}}</b><small>{{p.parkingType}}</small><em>Rs. {{p.fee}}</em>
              </div>
            }
          </div>
        }
      </section>
    </main>
  `,
  styles: [`
    .eyebrow{font-size:8px;font-weight:950;letter-spacing:1.6px;color:#0f8a78}.legend{display:flex;align-items:center;gap:11px;padding:8px 10px;border-radius:11px;background:#fff;border:1px solid #dce7e4}.legend span{display:flex;align-items:center;gap:5px;font-size:8px;font-weight:800;color:#52676c}.lg{width:9px;height:9px;border-radius:3px}
    .event-picker{padding:16px 18px;margin-bottom:14px;display:grid;grid-template-columns:minmax(260px,1fr) minmax(220px,.5fr);gap:18px;align-items:end}.event-picker label,.config-card label{display:flex;flex-direction:column;gap:6px;font-size:9px;font-weight:850;color:#41565b}.selected-info{display:flex;flex-direction:column;padding:4px 0 4px 18px;border-left:1px solid #e2ebe8}.selected-info small{font-size:8px;color:#7b8c90;text-transform:uppercase;letter-spacing:.8px}.selected-info b{font-size:11px;color:#18343a;margin-top:3px}
    .layout-columns{display:grid;grid-template-columns:1.1fr .9fr;gap:14px;margin-bottom:14px}.config-card{padding:18px}.config-head{display:flex;align-items:center;gap:10px;margin-bottom:15px}.config-icon{width:37px;height:37px;border-radius:11px;display:grid;place-items:center;color:#fff;font-size:11px;font-weight:950}.seat-icon{background:linear-gradient(135deg,#1d4ed8,#3b82f6)}.park-icon{background:linear-gradient(135deg,#6d28d9,#8b5cf6)}.config-head span,.map-head span{font-size:8px;color:#0f8a78;font-weight:950;letter-spacing:1.1px}.config-head h2,.map-head h2{margin:2px 0 0;font-size:15px}.form-grid{display:grid;grid-template-columns:repeat(5,1fr);gap:9px}.parking-form{grid-template-columns:repeat(2,1fr)}.action-row{display:flex;gap:8px;margin-top:14px}.outline{background:#fff;border:1px solid #dce7e4!important;color:#2b4146}.danger-btn{color:#b42318!important;border-color:#ffd4cf!important}
    .inventory-strip{display:grid;grid-template-columns:repeat(4,1fr);gap:12px;margin-bottom:14px}.inventory-card{position:relative;overflow:hidden;border-radius:15px;padding:15px 16px;background:#fff;border:1px solid #dce7e4;display:flex;flex-direction:column;box-shadow:0 10px 28px rgba(16,42,47,.045)}.inventory-card:before{content:"";position:absolute;left:0;top:0;bottom:0;width:4px}.available-card:before{background:#22a55b}.held-card:before{background:#e7a51a}.booked-card:before{background:#dc3d3d}.parking-card:before{background:#2563eb}.inventory-card span,.inventory-card small{font-size:8px;color:#728287}.inventory-card b{font-size:22px;color:#0b2330;margin:2px 0}
    .map-card{padding:18px;margin-bottom:14px}.map-head{display:flex;align-items:flex-start;justify-content:space-between;gap:10px;margin-bottom:15px}.map-head small{font-size:9px;color:#78898d}.stage{width:min(520px,70%);margin:0 auto 19px;padding:8px;border-radius:0 0 16px 16px;background:linear-gradient(90deg,#dbe7e4,#edf3f1,#dbe7e4);text-align:center;font-size:8px;font-weight:900;letter-spacing:1.3px;color:#718287}
    .seatmap{display:grid;grid-template-columns:repeat(auto-fill,minmax(82px,1fr));gap:8px}.seat,.parking-slot{min-height:58px;border-radius:10px;padding:8px;display:flex;flex-direction:column;justify-content:center;align-items:center;border:1px solid #dbe6e3;background:#f8faf9;transition:.12s}.seat b,.parking-slot b{font-size:9px}.seat small,.parking-slot small{font-size:7px;opacity:.75;margin:2px 0}.seat em,.parking-slot em{font-size:7px;font-style:normal;font-weight:800;opacity:.75}.available{background:#eaf8ef!important;border-color:#a8dfbb!important;color:#126b36!important}.held{background:#fff6d9!important;border-color:#f0cf71!important;color:#8b5a00!important}.booked{background:#ffebeb!important;border-color:#efaaaa!important;color:#b42318!important}.disabled{background:#eef1f2!important;border-color:#d4dadd!important;color:#718087!important}.parkmap{display:grid;grid-template-columns:repeat(auto-fill,minmax(96px,1fr));gap:8px}.parking-slot{min-height:66px}.empty-state{padding:38px 18px;text-align:center;border:1px dashed #cddbd8;border-radius:13px;background:#fafcfc;color:#78898d;font-size:10px}
    @media(max-width:1180px){.layout-columns{grid-template-columns:1fr}.form-grid{grid-template-columns:repeat(3,1fr)}}@media(max-width:850px){.event-picker{grid-template-columns:1fr}.selected-info{border-left:0;border-top:1px solid #e2ebe8;padding:10px 0 0}.inventory-strip{grid-template-columns:1fr 1fr}}@media(max-width:600px){.legend{flex-wrap:wrap}.form-grid,.parking-form{grid-template-columns:1fr 1fr}.inventory-strip{grid-template-columns:1fr 1fr}.seatmap{grid-template-columns:repeat(auto-fill,minmax(70px,1fr))}}
  `]
})
export class LayoutComponent {
  private api=inject(AdminApiService);readonly events=signal<any[]>([]);readonly seatRows=signal<any[]>([]);readonly parkRows=signal<any[]>([]);
  eventId=0;rows=10;cols=20;regularPrice=3500;premiumPrice=4500;vipPrice=6000;zone='B';parkingType='VIP';slots=20;fee=1000;
  constructor(){this.api.events().subscribe(x=>this.events.set(x))}
  load(){if(!this.eventId){this.seatRows.set([]);this.parkRows.set([]);return}this.loadSeats();this.loadParking()}
  private loadSeats(){this.api.seats(this.eventId).subscribe({next:x=>this.seatRows.set(x),error:()=>this.seatRows.set([])})}
  private loadParking(){this.api.parking(this.eventId).subscribe({next:x=>this.parkRows.set(x),error:()=>this.parkRows.set([])})}
  makeSeats(){if(!this.eventId){alert('Please select an event first.');return}if(this.seatRows().length>0){alert('A seat layout already exists. Use Reset available seats first if you need to regenerate it.');return}this.api.genSeats(this.eventId,{rows:this.rows,columns:this.cols,regularPrice:this.regularPrice,premiumPrice:this.premiumPrice,vipPrice:this.vipPrice}).subscribe({next:x=>{this.seatRows.set(x);alert(`Seats generated successfully: ${x.length}`)},error:e=>alert(e?.error?.message??'Seat generation failed')})}
  resetSeats(){if(!this.eventId){alert('Please select an event first.');return}const seats=this.seatRows();if(!seats.length){alert('There are no seats to reset.');return}const locked=seats.filter(x=>!this.statusIs(x.status,'Available'));if(locked.length){alert('Some seats are Held or Booked. Only a completely available layout can be reset.');return}if(!confirm('Delete all available seats for this event and regenerate later?'))return;from(seats).pipe(concatMap(s=>this.api.deleteSeat(this.eventId,s.seatId)),toArray()).subscribe({next:()=>{this.seatRows.set([]);alert('Old available seat layout removed.')},error:e=>alert(e?.error?.message??'Unable to reset seats.')})}
  makeOrAddParking(){if(!this.eventId){alert('Please select an event first.');return}const zone=this.zone.trim().toUpperCase();if(!zone){alert('Please enter a parking zone.');return}if(this.slots<=0){alert('Slot count must be greater than zero.');return}if(this.fee<0){alert('Parking fee cannot be negative.');return}if(this.parkRows().length===0){this.api.genParking(this.eventId,{zones:[{zone,parkingType:this.parkingType,slotCount:this.slots,fee:this.fee}]}).subscribe({next:x=>{this.parkRows.set(x);alert('Parking layout generated successfully.')},error:e=>alert(e?.error?.message??'Parking generation failed')});return}const existing=new Set(this.parkRows().map(p=>String(p.slotNumber||'').toUpperCase()));const newSlots=Array.from({length:this.slots},(_,i)=>({slotNumber:`${zone}-${String(i+1).padStart(3,'0')}`})).filter(x=>!existing.has(x.slotNumber));if(!newSlots.length){alert(`Zone ${zone} already contains these slots.`);return}from(newSlots).pipe(concatMap(x=>this.api.addParkingSlot(this.eventId,{zone,slotNumber:x.slotNumber,parkingType:this.parkingType,fee:this.fee})),toArray()).subscribe({next:()=>{this.loadParking();alert(`Zone ${zone} added successfully.`)},error:e=>{this.loadParking();alert(e?.error?.message??'Unable to add parking zone.')}})}
  statusIs(value:any,expected:string){return String(value||'').toLowerCase()===expected.toLowerCase()}
  availableSeats(){return this.seatRows().filter(x=>this.statusIs(x.status,'Available')).length}
  heldSeats(){return this.seatRows().filter(x=>this.statusIs(x.status,'Held')).length}
  bookedSeats(){return this.seatRows().filter(x=>this.statusIs(x.status,'Booked')).length}
  availableParking(){return this.parkRows().filter(x=>this.statusIs(x.status,'Available')&&!x.isDisabled).length}
}
