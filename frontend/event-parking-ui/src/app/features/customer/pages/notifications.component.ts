import { Component, inject, signal } from '@angular/core';
import { CustomerApiService } from '../customer-api.service';

@Component({
  selector:'app-notifications',
  template:`
    <main class="page notePage"><span class="kicker">CUSTOMER UPDATES</span><h1>Notification centre</h1><p>Booking, payment, reminder and update messages.</p>
      @if(error()){<div class="notice">{{error()}}</div>}
      @for(n of items();track n.notificationId){<article class="note" [class.unread]="!n.isRead" (click)="read(n)"><div class="icon">{{icon(n.type)}}</div><div><b>{{n.type}}</b><p>{{n.message}}</p><small>{{n.createdAt}}</small></div></article>}
      @empty{<div class="empty"><b>No notifications yet.</b><p>Booking and payment updates will appear here.</p></div>}
    </main>`,
  styles:[`
    .notePage{max-width:760px;padding:42px 0}.kicker{font-size:9px;letter-spacing:2px;color:#0b7a69;font-weight:900}.notePage h1{font-size:40px;letter-spacing:-1.5px;margin:7px 0;color:#07363a}.notePage>p,.note p,.note small,.empty p{color:#718184}.note{padding:16px;display:grid;grid-template-columns:44px 1fr;gap:12px;margin-bottom:12px;background:#fff;border:1px solid #dce7e4;border-radius:17px;box-shadow:0 12px 32px #07363a0b;cursor:pointer}.note.unread{border-left:4px solid #0b7a69}.icon{width:40px;height:40px;border-radius:50%;background:#e7f6f2;color:#0b7a69;display:grid;place-items:center;font-weight:900}.empty,.notice{padding:22px;border-radius:16px;margin-top:18px}.empty{background:#fff;border:1px dashed #cddbd8}.notice{background:#fff8e8;border:1px solid #f2d49a;color:#7c5710}
  `]
})
export class NotificationsComponent{
  private api=inject(CustomerApiService);
  readonly items=signal<any[]>([]);readonly error=signal('');
  constructor(){this.load()}
  load(){this.api.notifications().subscribe({next:x=>{this.items.set(x||[]);this.error.set('')},error:()=>{this.items.set([]);this.error.set('Could not load notifications. Start the backend and sign in as a Customer.')}})}
  read(n:any){if(!n.isRead)this.api.markRead(n.notificationId).subscribe({next:()=>this.load(),error:()=>this.error.set('Could not mark this notification as read.')})}
  icon(t:string){const x=(t||'').toLowerCase();return x.includes('payment')?'▣':x.includes('reminder')?'◷':'✓'}
}
