import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AdminApiService } from '../admin-api.service';

@Component({
  selector:'app-settings-admin',
  imports:[FormsModule],
  template:`
  <main class="pagex settings-page">
    <div class="head">
      <div><span class="eyebrow">PLATFORM CONFIGURATION</span><h1>Settings</h1><p>Manage the customer-facing site title, booking rules, parking, notifications and maintenance state.</p></div>
</div>

    @if(m();as x){
      <div class="settings-grid">
        <section class="card setting-card">
          <div class="setting-head"><span class="setting-icon brand">A</span><div><b>Brand & display</b><small>Customer-facing identity and money display.</small></div></div>
          <div class="field">
            <label>Site title / name</label>
            <input class="input" [(ngModel)]="x.siteName" placeholder="EventPark">
            <small>This uses the backend <b>SiteName</b> setting.</small>
          </div>
          <div class="field">
            <label>Currency</label>
            <input class="input" [(ngModel)]="x.currency" placeholder="LKR">
            <small>Example: LKR, USD. Keep it consistent with displayed prices.</small>
          </div>
        </section>

        <section class="card setting-card">
          <div class="setting-head"><span class="setting-icon booking">B</span><div><b>Booking rules</b><small>Control temporary seat hold behaviour.</small></div></div>
          <div class="field">
            <label>Seat hold duration</label>
            <div class="number-field"><input class="input" type="number" min="1" [(ngModel)]="x.holdMinutes"><span>minutes</span></div>
            <small>How long a customer can hold seats before completing checkout.</small>
          </div>

          <div class="toggle-row">
            <div><b>Allow parking reservations</b><small>Customers can select parking during booking.</small></div>
            <label class="switch"><input type="checkbox" [(ngModel)]="x.allowParking"><span></span></label>
          </div>
        </section>

        <section class="card setting-card">
          <div class="setting-head"><span class="setting-icon notify">N</span><div><b>Notifications</b><small>Communication behaviour for the platform.</small></div></div>
          <div class="toggle-row">
            <div><b>Email notifications</b><small>Enable application email notifications.</small></div>
            <label class="switch"><input type="checkbox" [(ngModel)]="x.emailNotificationsEnabled"><span></span></label>
          </div>
          <div class="info-box">Booking and notification flows continue to follow backend authorization and business rules.</div>
        </section>

        <section class="card setting-card danger-card">
          <div class="setting-head"><span class="setting-icon maintenance">M</span><div><b>Maintenance mode</b><small>System availability control.</small></div></div>
          <div class="toggle-row">
            <div><b>Enable maintenance mode</b><small>Use only when the application needs maintenance.</small></div>
            <label class="switch danger-switch"><input type="checkbox" [(ngModel)]="x.maintenanceMode"><span></span></label>
          </div>
          @if(x.maintenanceMode){<div class="warning-box">⚠ Maintenance mode is currently enabled in this form. Save to apply the backend setting.</div>}
        </section>
      </div>

      <section class="card save-bar">
        <div><b>Ready to update platform settings?</b><small>Changes are saved through the Admin Settings backend API.</small></div>
        <div class="save-actions">
          <button class="btn reload" (click)="reload()">Reload</button>
          <button class="btn primary" (click)="save(x)">Save settings</button>
        </div>
      </section>

      @if(msg()){<div class="toast" [class.error]="msgType()==='error'">{{msg()}}</div>}
    } @else {
      <div class="card loading">Loading settings from backend...</div>
    }
  </main>
  `,
  styles:[`
    .eyebrow{font-size:7px;font-weight:950;letter-spacing:1.5px;color:#11907b}
    .settings-grid{display:grid;grid-template-columns:1fr 1fr;gap:13px;margin-bottom:13px}.setting-card{padding:18px;min-height:210px}.setting-head{display:flex;align-items:center;gap:10px;padding-bottom:13px;margin-bottom:13px;border-bottom:1px solid #d8ebe6}.setting-icon{width:36px;height:36px;border-radius:11px;display:grid;place-items:center;color:white;font-size:9px;font-weight:950}.brand{background:linear-gradient(135deg,#176cc7,#428be0)}.booking{background:linear-gradient(135deg,#0b6d64,#1aad92)}.notify{background:linear-gradient(135deg,#6b37b7,#8c59d7)}.maintenance{background:linear-gradient(135deg,#b34c1f,#df762f)}.setting-head>div{display:flex;flex-direction:column}.setting-head b{font-size:13px}.setting-head small{font-size:9px;color:#72868b;margin-top:2px}
    .field{display:flex;flex-direction:column;gap:6px;margin-bottom:12px}.field label{font-size:11px;font-weight:900;color:#3f5b61}.field>small{font-size:9px;color:#75898e;line-height:1.45}.number-field{position:relative}.number-field input{width:100%;padding-right:70px!important}.number-field span{position:absolute;right:11px;top:50%;transform:translateY(-50%);font-size:7px;color:#74888d}
    .toggle-row{display:flex;align-items:center;justify-content:space-between;gap:15px;padding:12px;border-radius:11px;background:#f6fbfa;border:1px solid #d7ebe6;margin-bottom:10px}.toggle-row>div{display:flex;flex-direction:column}.toggle-row b{font-size:11px}.toggle-row small{font-size:9px;color:#74888d;margin-top:2px;line-height:1.4}
    .switch{position:relative;width:42px;height:23px;flex:0 0 42px}.switch input{opacity:0;width:0;height:0}.switch span{position:absolute;inset:0;border-radius:99px;background:#cbdad7;cursor:pointer;transition:.18s}.switch span:before{content:"";position:absolute;width:17px;height:17px;left:3px;top:3px;border-radius:50%;background:white;box-shadow:0 2px 5px #2448}.switch input:checked+span{background:#17a487}.switch input:checked+span:before{transform:translateX(19px)}.danger-switch input:checked+span{background:#dc4a3d}
    .info-box,.warning-box{padding:11px;border-radius:9px;font-size:9px;line-height:1.5}.info-box{background:#eef8f6;color:#527278;border:1px solid #d4ebe6}.warning-box{background:#fff0e9;color:#9b441f;border:1px solid #f3c3ae}.danger-card{border-color:#e6b6a4!important}
    .save-bar{padding:14px 17px;display:flex;align-items:center;justify-content:space-between;gap:15px}.save-bar>div:first-child{display:flex;flex-direction:column}.save-bar b{font-size:12px}.save-bar small{font-size:9px;color:#72868a;margin-top:2px}.save-actions{display:flex;gap:7px}.reload{background:white!important;color:#466167!important;border:1.4px solid #a5d5cc!important}
    .toast{margin-top:10px;padding:11px 13px;border-radius:10px;background:#e9f8ee;border:1px solid #a9ddba;color:#166d38;font-size:8px;font-weight:850}.toast.error{background:#fff0f0;border-color:#efb1b1;color:#b12626}.loading{padding:35px;text-align:center;color:#71858a}
    @media(max-width:850px){.settings-grid{grid-template-columns:1fr}.save-bar{align-items:flex-start;flex-direction:column}}
  `]
})
export class SettingsComponent{
  private api=inject(AdminApiService);
  readonly m=signal<any>(null);
  readonly msg=signal('');
  readonly msgType=signal<'ok'|'error'>('ok');
  constructor(){this.reload()}
  reload(){this.msg.set('');this.api.settings().subscribe({next:x=>this.m.set(x),error:e=>{this.msgType.set('error');this.msg.set(e?.error?.message??'Unable to load settings')}})}
  save(x:any){this.api.saveSettings(x).subscribe({next:r=>{this.m.set(r);this.msgType.set('ok');this.msg.set('Settings saved successfully.')},error:e=>{this.msgType.set('error');this.msg.set(e?.error?.message??'Save failed')}})}
}
