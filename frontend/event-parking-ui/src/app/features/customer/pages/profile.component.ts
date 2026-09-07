import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { CustomerApiService } from '../customer-api.service';

@Component({
  selector:'app-profile',imports:[FormsModule],
  template:`<main class="page wrap"><span class="kicker">ACCOUNT</span><h1>Profile</h1><p>Keep your customer details up to date.</p>
    @if(error()){<div class="notice">{{error()}}</div>}
    <section class="form"><label>Full name<input [(ngModel)]="m.fullName"></label><label>Email<input [value]="m.email||''" disabled></label><label>Phone<input [(ngModel)]="m.phone"></label><button (click)="save()">Save profile</button>@if(msg()){<p class="success">{{msg()}}</p>}</section>
  </main>`,
  styles:[`
    .wrap{padding:42px 0}.kicker{font-size:9px;letter-spacing:2px;color:#0b7a69;font-weight:900}.wrap h1{font-size:40px;letter-spacing:-1.5px;margin:7px 0;color:#07363a}.wrap>p{color:#718184}.form{max-width:560px;margin-top:22px;padding:24px;background:#fff;border:1px solid #dce7e4;border-radius:18px;box-shadow:0 14px 36px #07363a0c;display:flex;flex-direction:column;gap:14px}.form label{display:flex;flex-direction:column;gap:7px;font-size:12px;font-weight:800;color:#33494c}.form input{min-height:44px;border:1px solid #d8e4e1;border-radius:11px;padding:0 12px}.form button{min-height:45px;border:0;border-radius:11px;background:#07363a;color:#fff;font-weight:900;cursor:pointer}.success{color:#15803d}.notice{max-width:560px;margin-top:18px;padding:13px 15px;border-radius:13px;background:#fff8e8;border:1px solid #f2d49a;color:#7c5710}
  `]
})
export class ProfileComponent{
  private api=inject(CustomerApiService);private a=inject(AuthService);
  m:any={fullName:'',email:'',phone:''};readonly msg=signal('');readonly error=signal('');
  constructor(){this.api.me().subscribe({next:x=>{this.m=x||this.m;this.error.set('')},error:()=>this.error.set('Could not load your profile. Start the backend and sign in as a Customer.')})}
  save(){const id=this.a.user()?.customerId;if(!id){this.error.set('Customer login is required.');return}this.api.updateMe(id,{fullName:this.m.fullName,phone:this.m.phone}).subscribe({next:()=>{this.msg.set('Profile updated');this.error.set('')},error:e=>this.error.set(e?.error?.message??'Update failed')})}
}
