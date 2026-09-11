import { Component, inject, signal } from '@angular/core';
import { CustomerApiService } from '../customer-api.service';

@Component({
  selector:'app-notifications',
  template:`
    <main class="page notePage">
      <div class="head"><div><span class="kicker">CUSTOMER UPDATES</span><h1>Notification centre</h1><p>Booking, payment, reminder and EventPark update messages.</p></div><div class="count"><small>Unread</small><b>{{unread()}}</b></div></div>
      @if(error()){<div class="notice">{{error()}}</div>}
      <div class="notes">
        @for(n of items();track n.notificationId){
          <article class="note" [class.unread]="!n.isRead" (click)="read(n)">
            <div class="icon">{{icon(n.type)}}</div>
            <div class="copy"><div><b>{{n.type}}</b>@if(!n.isRead){<span>NEW</span>}</div><p>{{n.message}}</p><small>{{dateText(n.createdAt)}}</small></div>
            <span class="arrow">›</span>
          </article>
        } @empty {
          <div class="empty"><div>✓</div><b>You're all caught up.</b><p>Booking and payment updates will appear here.</p></div>
        }
      </div>
    </main>
  `,
  styles:[`
    .notePage{max-width:860px;padding:42px 0}.head{display:flex;justify-content:space-between;align-items:center;gap:16px}.kicker{font-size:9px;letter-spacing:1.8px;color:#0e8f79;font-weight:950}.head h1{font-size:39px;letter-spacing:-1.4px;margin:6px 0;color:#07383a}.head p{margin:0;color:#6c8287}.count{min-width:90px;padding:11px 13px;border:1px solid #9fd4ca;border-radius:12px;background:#ffffffdf;display:flex;flex-direction:column}.count small{font-size:7px;color:#74898d}.count b{font-size:20px;color:#0e8f79}
    .notes{display:flex;flex-direction:column;gap:9px;margin-top:21px}.note{padding:14px;display:grid;grid-template-columns:42px 1fr auto;gap:12px;align-items:center;background:#ffffffeb;border:1px solid #addbd2;border-radius:15px;box-shadow:0 11px 28px #07383a0a;cursor:pointer;transition:.15s}.note:hover{transform:translateX(2px);border-color:#67beae}.note.unread{border-left:4px solid #0e8f79;background:#fbfffe}.icon{width:40px;height:40px;border-radius:11px;background:#e8f7f3;color:#0e8f79;display:grid;place-items:center;font-weight:950}.copy>div{display:flex;align-items:center;gap:7px}.copy b{font-size:10px;color:#24464b}.copy>div span{padding:4px 6px;border-radius:99px;background:#fff0d0;color:#9b6500;font-size:6px;font-weight:950}.copy p{font-size:9px;color:#637c81;margin:5px 0}.copy small{font-size:7px;color:#8a989c}.arrow{font-size:22px;color:#78a59d}.empty,.notice{padding:25px;border-radius:16px;margin-top:18px}.empty{text-align:center;background:#ffffffdc;border:1px dashed #9fcfc6}.empty>div{width:46px;height:46px;border-radius:50%;background:#e8f7ed;color:#168044;display:grid;place-items:center;margin:0 auto 8px}.empty p{color:#71858a}.notice{background:#fff8e8;border:1px solid #efcf91;color:#805a13;font-size:9px}
  `]
})
export class NotificationsComponent{
  private api=inject(CustomerApiService);
  readonly items=signal<any[]>([]);
  readonly error=signal('');
  constructor(){this.load()}
  load(){this.api.notifications().subscribe({next:x=>{this.items.set(x||[]);this.error.set('')},error:()=>{this.items.set([]);this.error.set('Could not load notifications. Make sure the backend is running and you are signed in as a Customer.')}})}
  read(n:any){if(!n.isRead)this.api.markRead(n.notificationId).subscribe({next:()=>this.load(),error:()=>this.error.set('Could not mark this notification as read.')})}
  unread(){return this.items().filter(x=>!x.isRead).length}
  icon(t:string){const x=(t||'').toLowerCase();return x.includes('payment')?'Rs':x.includes('reminder')?'◷':x.includes('cancel')?'×':'✓'}
  dateText(v:any){if(!v)return '';const d=new Date(v);return Number.isNaN(d.getTime())?String(v):d.toLocaleString('en-GB')}
}
