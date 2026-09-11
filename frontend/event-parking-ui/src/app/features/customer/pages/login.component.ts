import { Component,inject,signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute,Router,RouterLink } from '@angular/router';
import { Location } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';

type LoginMode = 'choose' | 'customer' | 'admin';

@Component({
  selector:'app-login',
  imports:[FormsModule,RouterLink],
  template:`
    <section class="auth-shell"
      [class.admin-mode]="mode()==='admin'"
      [class.customer-mode]="mode()==='customer'">

      <div class="visual-panel">
        <div class="visual-overlay"></div>

        <div class="brand-row">
          <a routerLink="/" class="brand">
            <img src="/customer-assets/eventpark-logo-light.svg" alt="EventPark">
          </a>
          <span class="mini-badge">EVENT + PARKING</span>
        </div>

        <div class="visual-copy">
          <span class="eyebrow">EVENTPARK ACCESS</span>

          @if(mode()==='choose'){
            <h2>One platform.<br><em>Two experiences.</em></h2>
            <p>Choose how you want to enter EventPark — as a customer or as an administrator.</p>
          } @else if(mode()==='admin'){
            <h2>Manage.<br><em>Monitor.</em><br>Operate.</h2>
            <p>Control events, bookings, parking operations, customers and reports from the Admin Console.</p>
          } @else {
            <h2>Reserve.<br><em>Arrive.</em><br>Enjoy.</h2>
            <p>Discover events, reserve seats, add parking and keep your full EventPark journey in one place.</p>
          }
        </div>

        <div class="image-wrap">
          <img src="/customer-assets/auth-event-parking-final.jpg" alt="Event venue parking">
          <div class="image-note">
            <small>{{mode()==='admin' ? 'ADMIN CONTROL' : mode()==='customer' ? 'CUSTOMER JOURNEY' : 'EVENTPARK'}}</small>
            <b>
              {{mode()==='admin'
                ? 'Everything you need to operate EventPark.'
                : mode()==='customer'
                ? 'Less parking stress. More event moments.'
                : 'Choose your access and continue.'}}
            </b>
          </div>
        </div>

        <div class="benefits">
          @if(mode()==='admin'){
            <article><span>EV</span><div><b>Events</b><small>Manage operations</small></div></article>
            <article><span>BK</span><div><b>Bookings</b><small>Track activity</small></div></article>
            <article><span>RP</span><div><b>Reports</b><small>View insights</small></div></article>
          } @else {
            <article><span>◫</span><div><b>Book</b><small>Events & seats</small></div></article>
            <article><span>P</span><div><b>Park</b><small>Reserve parking</small></div></article>
            <article><span>QR</span><div><b>Enter</b><small>Your digital ticket</small></div></article>
          }
        </div>
      </div>

      <div class="form-side">
        <a routerLink="/" class="back-home">← Back to home</a>

        <div class="login-card">
          <a routerLink="/" class="form-logo">
            <img src="/customer-assets/eventpark-logo.svg" alt="EventPark">
          </a>

          @if(mode()==='choose'){
            <div class="choose-stage">
              <span class="kicker">WELCOME TO EVENTPARK</span>
              <h1>Choose your login</h1>
              <p>Select Customer or Admin to open the correct login form.</p>

              <div class="role-picker">
                <button type="button" class="role-card customer-card" (click)="selectMode('customer')">
                  <span class="role-icon">👤</span>
                  <div>
                    <b>Customer Login</b>
                    <small>Events • bookings • parking • QR tickets</small>
                  </div>
                  <strong>→</strong>
                </button>

                <button type="button" class="role-card admin-card" (click)="selectMode('admin')">
                  <span class="role-icon">🛡</span>
                  <div>
                    <b>Admin Login</b>
                    <small>Events • customers • reports • operations</small>
                  </div>
                  <strong>→</strong>
                </button>
              </div>

              <div class="choose-note">
                <span>🔐</span>
                <div>
                  <b>Secure role-based access</b>
                  <small>Customer accounts and Admin accounts remain separate.</small>
                </div>
              </div>

              <p class="foot">New customer? <a routerLink="/register">Create an account →</a></p>
            </div>
          }

          @if(mode()!=='choose'){
            <div class="form-stage">
              <button class="change-role" type="button" (click)="backToChoose()">
                ← Change login type
              </button>

              <div class="mode-chip" [class.admin-chip]="mode()==='admin'">
                <span>{{mode()==='admin' ? '🛡' : '👤'}}</span>
                <div>
                  <small>YOU ARE SIGNING IN AS</small>
                  <b>{{mode()==='admin' ? 'Administrator' : 'Customer'}}</b>
                </div>
              </div>

              <span class="kicker">{{mode()==='admin' ? 'ADMIN ACCESS' : 'CUSTOMER ACCESS'}}</span>
              <h1>{{mode()==='admin' ? 'Open Admin Console' : 'Welcome back'}}</h1>
              <p>
                {{mode()==='admin'
                  ? 'Use your administrator account to access EventPark operations.'
                  : 'Sign in to continue to your bookings, events and tickets.'}}
              </p>

              <form (ngSubmit)="go()">
                <label>Email address
                  <div class="field">
                    <span>✉</span>
                    <input [(ngModel)]="email" name="email" type="email" autocomplete="email" placeholder="you@example.com">
                  </div>
                </label>

                <div class="password-head">
                  <label>Password</label>
                  <a [routerLink]="['/forgot-password']" [queryParams]="{role:mode()}">Forgot password?</a>
                </div>

                <div class="field">
                  <span>▣</span>
                  <input [(ngModel)]="password" name="password"
                         [type]="showPassword() ? 'text' : 'password'"
                         autocomplete="current-password" placeholder="Enter your password">
                  <button class="eye" type="button" (click)="showPassword.set(!showPassword())">
                    {{showPassword() ? 'Hide' : 'Show'}}
                  </button>
                </div>

                @if(error()){<div class="error">{{error()}}</div>}
                @if(loading()){<div class="loading-note">Checking account and opening the correct workspace...</div>}

                <button class="sign-btn" [class.admin-btn]="mode()==='admin'" [disabled]="loading()">
                  {{loading() ? 'Signing in...' : (mode()==='admin' ? 'Sign in as Admin' : 'Sign in as Customer')}}
                  <span>→</span>
                </button>
              </form>

              <div class="mini-switch">
                <span>{{mode()==='admin' ? 'Not an admin?' : 'Need admin access?'}}</span>
                <button type="button" (click)="selectMode(mode()==='admin' ? 'customer' : 'admin')">
                  Switch to {{mode()==='admin' ? 'Customer' : 'Admin'}}
                </button>
              </div>

              @if(mode()==='customer'){
                <p class="foot">Don't have an account? <a routerLink="/register">Create customer account →</a></p>
              } @else {
                <div class="admin-note">Administrator accounts are managed by the EventPark system.</div>
              }
            </div>
          }
        </div>
      </div>
    </section>
  `,
  styles:[`
    :host{
      display:block;
      --navy:#062f3c;
      --blue:#1677ff;
      --teal:#0b927d;
      --cyan:#57c9ff;
      --muted:#6f8589;
    }

    .auth-shell{
      min-height:100vh;
      display:grid;
      grid-template-columns:1fr 1fr;
      overflow:hidden;
      background:
        radial-gradient(circle at 83% 18%,#d7ebff 0,transparent 24%),
        linear-gradient(135deg,#eaf4ff,#f8fbff 52%,#edf9f6);
    }

    .visual-panel{
      position:relative;
      overflow:hidden;
      padding:34px 44px;
      color:#fff;
      background:
        linear-gradient(145deg,rgba(3,31,55,.97),rgba(6,51,80,.93) 52%,rgba(7,90,94,.91)),
        url('/customer-assets/customer-operations-bg.png') center/cover;
      display:flex;
      flex-direction:column;
    }

    .visual-overlay{
      position:absolute;
      inset:0;
      pointer-events:none;
      background:
        radial-gradient(circle at 12% 82%,rgba(22,119,255,.18),transparent 27%),
        radial-gradient(circle at 86% 18%,rgba(11,146,125,.20),transparent 30%);
    }

    .brand-row{
      position:relative;z-index:2;
      display:flex;align-items:center;justify-content:space-between;gap:12px
    }

    .brand img{width:165px;display:block}

    .mini-badge{
      padding:7px 10px;border:1px solid #84d3ff2c;border-radius:999px;
      background:#ffffff0c;color:#a5dcff;font-size:8px;font-weight:950;letter-spacing:1px
    }

    .visual-copy{position:relative;z-index:2;margin:28px 0 18px;max-width:610px}
    .eyebrow,.kicker{font-size:10px;letter-spacing:1.8px;font-weight:950;color:var(--cyan)}
    .visual-copy h2{font-size:56px;line-height:.93;letter-spacing:-2.7px;margin:10px 0;color:#fff}
    .visual-copy h2 em{font-style:normal;color:#58dfc3}
    .visual-copy p{font-size:15px;line-height:1.65;color:#c7dce5;max-width:540px;margin:0}

    .image-wrap{
      position:relative;z-index:2;border-radius:21px;overflow:hidden;
      border:1px solid #8bd4ff33;box-shadow:0 24px 52px #01172465
    }

    .image-wrap img{
      width:100%;height:340px;display:block;object-fit:cover;object-position:center;
      filter:saturate(1.06) contrast(1.04)
    }

    .image-wrap:after{
      content:"";position:absolute;inset:0;
      background:linear-gradient(180deg,transparent 45%,rgba(2,26,44,.70))
    }

    .image-note{
      position:absolute;z-index:2;left:15px;bottom:15px;padding:10px 12px;
      border-radius:11px;background:#062f3de4;border:1px solid #ffffff1b;
      display:flex;flex-direction:column;gap:2px
    }

    .image-note small{font-size:7px;letter-spacing:1px;color:#67caff;font-weight:950}
    .image-note b{font-size:11px;line-height:1.35}

    .benefits{
      position:relative;z-index:2;
      display:grid;grid-template-columns:repeat(3,1fr);gap:9px;margin-top:12px
    }

    .benefits article{
      padding:11px;border-radius:12px;background:#ffffff0c;border:1px solid #77d0ff20;
      display:flex;align-items:center;gap:8px
    }

    .benefits article>span{
      width:32px;height:32px;border-radius:9px;display:grid;place-items:center;
      background:linear-gradient(135deg,var(--blue),var(--teal));
      font-size:9px;font-weight:950
    }

    .benefits div{display:flex;flex-direction:column}
    .benefits b{font-size:11px}
    .benefits small{font-size:8px;color:#b9d0d9}

    .form-side{
      position:relative;
      display:grid;
      place-items:center;
      padding:60px 34px 30px;
      background:linear-gradient(145deg,rgba(255,255,255,.99),rgba(244,250,255,.97))
    }

    .form-side:before{
      content:"";position:absolute;inset:0;
      background:url('/customer-assets/customer-operations-bg.png') center/cover;
      opacity:.055;pointer-events:none
    }

    .back-home{
      position:absolute;right:28px;top:22px;z-index:3;
      color:#607883;text-decoration:none;font-size:11px;font-weight:850
    }

    .login-card{
      position:relative;z-index:2;
      width:min(525px,100%);
      min-height:620px;
      padding:30px 33px;
      background:rgba(255,255,255,.96);
      border:1px solid #b7d4e5;
      border-radius:25px;
      box-shadow:0 30px 75px #0a46711c;
      backdrop-filter:blur(16px);
      display:flex;
      flex-direction:column;
      justify-content:center;
    }

    .form-logo{display:flex;justify-content:center;margin-bottom:14px}
    .form-logo img{width:170px}

    .kicker{display:block;text-align:center;color:var(--blue);margin-bottom:7px}

    .choose-stage h1,.form-stage h1{
      font-size:36px;line-height:1.05;letter-spacing:-1.4px;
      text-align:center;margin:0;color:var(--navy)
    }

    .choose-stage>p,.form-stage>p{
      text-align:center;color:var(--muted);
      font-size:13px;line-height:1.6;margin:9px 0 18px
    }

    .role-picker{
      display:grid;grid-template-columns:1fr 1fr;gap:10px;margin-bottom:15px
    }

    .role-card{
      min-height:150px;
      border:1px solid #cfe0e9;
      border-radius:16px;
      background:#fff;
      padding:16px;
      text-align:left;
      display:flex;
      flex-direction:column;
      justify-content:space-between;
      gap:8px;
      cursor:pointer;
    }

    .role-card:hover{
      border-color:#8cc6e7;
      box-shadow:0 12px 24px #0b4d7012;
    }

    .role-icon{
      width:43px;height:43px;border-radius:12px;
      display:grid;place-items:center;
      background:#eaf4ff;color:var(--blue);
      font-size:17px;
    }

    .admin-card .role-icon{background:#f0edff;color:#6848e4}
    .role-card div{display:flex;flex-direction:column;gap:3px}
    .role-card b{font-size:13px;color:#284b55}
    .role-card small{font-size:8px;line-height:1.45;color:#778d93}
    .role-card strong{font-size:18px;color:#6c8790}

    .choose-note{
      padding:11px 12px;border-radius:12px;
      background:#f4f9fc;border:1px solid #d5e5ee;
      display:flex;gap:9px;align-items:flex-start
    }

    .choose-note>span{font-size:15px}
    .choose-note div{display:flex;flex-direction:column;gap:2px}
    .choose-note b{font-size:10px;color:#264954}
    .choose-note small{font-size:8px;line-height:1.5;color:#73888f}

    .change-role{
      border:0;background:transparent;padding:0;margin-bottom:12px;
      color:#0a66c7;font-size:10px;font-weight:900;cursor:pointer;text-align:left
    }

    .mode-chip{
      display:flex;align-items:center;gap:9px;padding:10px 12px;margin-bottom:13px;
      border-radius:12px;
      background:linear-gradient(135deg,#f0f7ff,#f0fbf8);
      border:1px solid #cfe1ed
    }

    .mode-chip.admin-chip{
      background:linear-gradient(135deg,#eff4ff,#f5f0ff);
      border-color:#d4d1f6;
    }

    .mode-chip>span{
      width:36px;height:36px;border-radius:10px;
      display:grid;place-items:center;
      background:linear-gradient(135deg,var(--blue),var(--teal));
      color:#fff
    }

    .admin-chip>span{background:linear-gradient(135deg,#0b3d72,#7b5cff)}
    .mode-chip div{display:flex;flex-direction:column}
    .mode-chip small{font-size:7px;letter-spacing:.8px;color:#7a8f95;font-weight:900}
    .mode-chip b{font-size:11px;color:#254953}

    .form-stage form{
      display:flex;flex-direction:column;gap:12px;margin-top:4px
    }

    .form-stage label,.password-head label{
      font-size:11px;font-weight:850;color:#35555b
    }

    .password-head{
      display:flex;align-items:center;justify-content:space-between;margin-bottom:-6px
    }

    .password-head a{
      font-size:9px;color:#0a66c7;text-decoration:none;font-weight:900
    }

    .field{
      display:flex;align-items:center;gap:9px;
      min-height:48px;padding:0 11px;
      border:1px solid #bdd7e5;border-radius:11px;background:#fff
    }

    .field:focus-within{
      border-color:#4e9fe2;box-shadow:0 0 0 3px #1677ff12
    }

    .field>span{color:#66838f}
    .field input{flex:1;border:0;outline:0;background:transparent;font-size:13px;color:#17373d}

    .eye{
      border:0;background:transparent;color:var(--blue);
      font-size:9px;font-weight:950;cursor:pointer
    }

    .sign-btn{
      min-height:49px;border:0;border-radius:13px;
      background:linear-gradient(135deg,#0b66dc,#1677ff 62%,#0b927d);
      color:#fff;font-size:13px;font-weight:950;cursor:pointer;
      display:flex;align-items:center;justify-content:center;gap:10px;
      box-shadow:0 13px 27px #1677ff26
    }

    .sign-btn.admin-btn{
      background:linear-gradient(135deg,#0b3d72,#4169c8 58%,#7b5cff);
      box-shadow:0 13px 27px rgba(79,73,204,.22)
    }

    .sign-btn span{font-size:19px}
    .sign-btn:disabled{opacity:.6;cursor:wait}

    .error,.loading-note,.admin-note{
      padding:10px 11px;border-radius:10px;font-size:10px;line-height:1.5
    }

    .error{background:#fff0f0;border:1px solid #efb4b4;color:#b42626}
    .loading-note{background:#edf6ff;border:1px solid #bad9f3;color:#135e9e;text-align:center}
    .admin-note{margin-top:12px;background:#f1f6fa;border:1px solid #d5e3eb;color:#60747b;text-align:center}

    .mini-switch{
      margin-top:13px;padding-top:11px;border-top:1px solid #e0ebf0;
      display:flex;align-items:center;justify-content:center;gap:7px;
      font-size:9px;color:#768b92
    }

    .mini-switch button{
      border:0;background:transparent;color:var(--blue);
      font-size:9px;font-weight:950;cursor:pointer
    }

    .foot{text-align:center!important;margin:13px 0 0!important;font-size:10px!important;color:#7a8d92}
    .foot a{color:#0a66c7;text-decoration:none;font-weight:950}

    @media(max-width:900px){
      .auth-shell{grid-template-columns:1fr}
      .visual-panel{display:none}
      .form-side{min-height:100vh;padding:56px 18px 22px}
      .login-card{min-height:auto;padding:26px}
      .back-home{right:18px}
    }

    @media(max-width:520px){
      .role-picker{grid-template-columns:1fr}
      .role-card{min-height:115px}
    }
  `]
})
export class LoginComponent{
  private auth=inject(AuthService);
  private route=inject(ActivatedRoute);
  private location=inject(Location);

  readonly mode=signal<LoginMode>(
    this.route.snapshot.data['loginRole']==='admin'
      ? 'admin'
      : this.route.snapshot.data['loginRole']==='customer'
        ? 'customer'
        : 'choose'
  );

  readonly error=signal('');
  readonly loading=signal(false);
  readonly showPassword=signal(false);

  email='';
  password='';

  selectMode(next:'customer'|'admin'){
    this.error.set('');
    this.password='';
    this.mode.set(next);
    this.location.replaceState(next==='admin' ? '/login/admin' : '/login/customer');
  }

  backToChoose(){
    this.mode.set('choose');
    this.location.replaceState('/login');
    this.error.set('');
    this.password='';
  }

  go(){
    this.error.set('');

    if(this.mode()==='choose'){
      this.error.set('Choose Customer Login or Admin Login first.');
      return;
    }

    if(!this.email.trim() || !this.password){
      this.error.set('Enter your email and password.');
      return;
    }

    this.loading.set(true);

    this.auth.login({email:this.email.trim(),password:this.password}).subscribe({
      next:(r:any)=>{
        const actual=String(r?.customer?.role||'').toLowerCase();
        const expected=this.mode();

        if(actual!==expected){
          this.auth.clearSession();
          this.loading.set(false);
          this.error.set(
            expected==='admin'
              ? 'This is not an Admin account. Please use Customer Login.'
              : 'This is an Admin account. Please use Admin Login.'
          );
          return;
        }

        this.auth.goAfterLogin();
      },
      error:e=>{
        this.loading.set(false);
        this.error.set(e?.error?.message??'Login failed');
      }
    })
  }
}
