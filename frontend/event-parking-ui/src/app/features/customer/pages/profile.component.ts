import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { CustomerApiService } from '../customer-api.service';

@Component({
  selector:'app-profile',
  imports:[FormsModule],
  template:`
  <main class="page wrap">
    <div class="head">
      <div><span class="kicker">ACCOUNT</span><h1>Your profile</h1><p>Manage customer information and review your account activity.</p></div>
      <div class="identity"><span>{{initial()}}</span><div><b>{{m.fullName || 'Customer'}}</b><small>{{m.role || 'Customer'}} account</small></div></div>
    </div>

    @if(error()){<div class="notice">{{error()}}</div>}

    <div class="profile-grid">
      <section class="card form-card">
        <div class="section-title"><div><span>PERSONAL DETAILS</span><h2>Profile information</h2></div><small>Fields marked editable can be updated.</small></div>

        <label>Full name
          <input [(ngModel)]="m.fullName" placeholder="Your full name">
        </label>

        <label>Account email
          <div class="email-field">
            <input [value]="m.email || ''" readonly>
            <span [class.verified]="m.emailVerified">{{m.emailVerified ? '✓ Verified' : 'Account email'}}</span>
          </div>
          <small class="helper">Your current backend uses email as the login identity, so email is displayed safely as read-only here.</small>
        </label>

        <label>Phone
          <input [(ngModel)]="m.phone" placeholder="0771234567">
        </label>

        <div class="save-row">
          <span>Only name and phone are updated by the current Customer Profile API.</span>
          <button (click)="save()" [disabled]="saving()">{{saving() ? 'Saving...' : 'Save profile'}}</button>
        </div>

        @if(msg()){<div class="success">✓ {{msg()}}</div>}
      </section>

      <aside class="side">
        <section class="card account-card">
          <span class="mini-title">ACCOUNT STATUS</span>
          <div class="status-line"><span class="status-dot"></span><div><b>{{m.status || 'Active'}}</b><small>Customer account</small></div></div>
          <div class="info-line"><span>Email verification</span><b>{{m.emailVerified ? 'Verified' : 'Pending'}}</b></div>
          <div class="info-line"><span>Member since</span><b>{{dateText(m.createdAt)}}</b></div>
        </section>

        <section class="card activity-card">
          <span class="mini-title">BOOKING ACTIVITY</span>
          <div class="activity-grid">
            <div><b>{{summary().totalBookings || 0}}</b><small>Total bookings</small></div>
            <div><b>{{summary().upcomingBookings || 0}}</b><small>Upcoming</small></div>
            <div><b>{{summary().cancelledBookings || 0}}</b><small>Cancelled</small></div>
          </div>
        </section>

        <section class="card privacy-card">
          <span class="mini-title">ACCOUNT TIP</span>
          <b>Keep your contact details current.</b>
          <p>Your phone can be updated here. Email changes need backend verification support before they should be enabled in the UI.</p>
        </section>
      </aside>
    </div>
  </main>
  `,
  styles:[`
    .wrap{padding:42px 0}.head{display:flex;justify-content:space-between;align-items:center;gap:18px}.kicker,.mini-title{font-size:8px;letter-spacing:1.6px;color:#0e8f79;font-weight:950}.head h1{font-size:39px;letter-spacing:-1.4px;margin:6px 0;color:#07383a}.head p{margin:0;color:#6c8287}.identity{display:flex;align-items:center;gap:9px;padding:8px 11px;border:1px solid #9fd4ca;border-radius:13px;background:#ffffffdf}.identity>span{width:38px;height:38px;border-radius:11px;background:linear-gradient(135deg,#07383a,#0e8f79);color:#fff;display:grid;place-items:center;font-weight:950}.identity>div{display:flex;flex-direction:column}.identity b{font-size:10px}.identity small{font-size:7px;color:#71858a}
    .profile-grid{display:grid;grid-template-columns:1.15fr .85fr;gap:14px;margin-top:21px}.card{background:#ffffffeb;border:1px solid #9fd4ca;border-radius:17px;box-shadow:0 14px 34px #07383a0b}.form-card{padding:20px;display:flex;flex-direction:column;gap:13px}.section-title{display:flex;justify-content:space-between;align-items:flex-start;padding-bottom:13px;border-bottom:1px solid #d9ebe7}.section-title span{font-size:7px;color:#0e8f79;letter-spacing:1.2px;font-weight:950}.section-title h2{font-size:17px;margin:3px 0 0;color:#17383d}.section-title>small{font-size:7px;color:#7d9094}
    .form-card label{display:flex;flex-direction:column;gap:6px;font-size:9px;font-weight:900;color:#3b5a60}.form-card input{min-height:43px;border:1px solid #addbd2;border-radius:10px;padding:0 11px;background:#fff;outline:0}.form-card input:focus{border-color:#0e8f79;box-shadow:0 0 0 3px #0e8f7915}.email-field{position:relative}.email-field input{width:100%;box-sizing:border-box;padding-right:90px;background:#f4faf8}.email-field span{position:absolute;right:9px;top:50%;transform:translateY(-50%);padding:5px 7px;border-radius:99px;background:#edf1f1;color:#66777c;font-size:7px}.email-field span.verified{background:#e8f7ed;color:#147a3c}.helper{font-size:7px!important;color:#788c90;font-weight:500!important;line-height:1.5}
    .save-row{display:flex;justify-content:space-between;align-items:center;gap:15px;padding-top:6px}.save-row>span{font-size:7px;color:#7a8d91;max-width:310px}.save-row button{min-height:42px;border:0;border-radius:10px;padding:0 15px;background:linear-gradient(135deg,#07383a,#0e8f79);color:#fff;font-size:9px;font-weight:950;cursor:pointer}.save-row button:disabled{opacity:.55}.success,.notice{padding:11px 13px;border-radius:10px;font-size:9px}.success{background:#e8f7ed;border:1px solid #b8e0c4;color:#176d39}.notice{margin-top:15px;background:#fff1f1;border:1px solid #efb3b3;color:#a92a2a}
    .side{display:flex;flex-direction:column;gap:12px}.account-card,.activity-card,.privacy-card{padding:17px}.status-line{display:flex;align-items:center;gap:9px;margin:12px 0}.status-dot{width:10px;height:10px;border-radius:50%;background:#27be6a;box-shadow:0 0 0 4px #27be6a18}.status-line>div{display:flex;flex-direction:column}.status-line b{font-size:11px}.status-line small{font-size:7px;color:#71858a}.info-line{display:flex;justify-content:space-between;padding:9px 0;border-top:1px solid #dceae7;font-size:8px;color:#70858a}.info-line b{color:#28474c}.activity-grid{display:grid;grid-template-columns:repeat(3,1fr);gap:7px;margin-top:12px}.activity-grid div{padding:11px 7px;border-radius:11px;background:#f1faf7;border:1px solid #d4ebe5;display:flex;flex-direction:column;text-align:center}.activity-grid b{font-size:18px;color:#0e8f79}.activity-grid small{font-size:7px;color:#758a8e}.privacy-card>b{display:block;font-size:11px;margin:10px 0 5px}.privacy-card p{font-size:8px;color:#71858a;line-height:1.6;margin:0}
    @media(max-width:780px){.profile-grid{grid-template-columns:1fr}.head{align-items:flex-start;flex-direction:column}}@media(max-width:520px){.save-row{align-items:flex-start;flex-direction:column}.activity-grid{grid-template-columns:1fr}}
  `]
})
export class ProfileComponent{
  private api=inject(CustomerApiService);
  private auth=inject(AuthService);
  m:any={fullName:'',email:'',phone:'',role:'Customer',status:'Active',emailVerified:false,createdAt:null};
  readonly summary=signal<any>({totalBookings:0,upcomingBookings:0,cancelledBookings:0});
  readonly msg=signal('');
  readonly error=signal('');
  readonly saving=signal(false);

  constructor(){this.load()}

  load(){
    this.api.me().subscribe({
      next:(x:any)=>{
        // /customers/me returns { profile, totalBookings, upcomingBookings, cancelledBookings }.
        // Older UI treated the wrapper itself as the profile, which made email/name appear broken.
        const p=x?.profile??x??{};
        this.m={...this.m,...p};
        this.summary.set({
          totalBookings:x?.totalBookings??0,
          upcomingBookings:x?.upcomingBookings??0,
          cancelledBookings:x?.cancelledBookings??0
        });
        this.error.set('')
      },
      error:()=>this.error.set('Could not load your profile. Make sure the backend is running and you are signed in as a Customer.')
    })
  }

  save(){
    const id=this.auth.user()?.customerId;
    if(!id){this.error.set('Customer login is required.');return}
    if(!String(this.m.fullName||'').trim()){this.error.set('Full name is required.');return}
    this.saving.set(true);this.msg.set('');this.error.set('');
    this.api.updateMe(id,{fullName:String(this.m.fullName).trim(),phone:String(this.m.phone||'').trim()||null}).subscribe({
      next:(p:any)=>{this.m={...this.m,...p};this.saving.set(false);this.msg.set('Profile updated successfully.')},
      error:e=>{this.saving.set(false);this.error.set(e?.error?.message??'Profile update failed.')}
    })
  }

  initial(){return String(this.m.fullName||this.auth.user()?.fullName||'C').trim().charAt(0).toUpperCase()}
  dateText(v:any){if(!v)return '-';const d=new Date(v);return Number.isNaN(d.getTime())?String(v):d.toLocaleDateString('en-GB',{day:'2-digit',month:'short',year:'numeric'})}
}
