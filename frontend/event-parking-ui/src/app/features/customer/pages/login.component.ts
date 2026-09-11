import { Component,inject,signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector:'app-login',
  imports:[FormsModule,RouterLink],
  template:`
    <section class="auth-shell">
      <div class="visual-panel">
        <div class="brand-row">
          <a routerLink="/" class="brand"><img src="/customer-assets/eventpark-logo-light.svg" alt="EventPark"></a>
        </div>

        <div class="visual-copy">
          <span class="eyebrow">SMART EVENT PARKING EXPERIENCE</span>
          <h2>Great Events<br>Start with<br><em>Easy Parking</em></h2>
          <p>Find. Book. Arrive. Enjoy.</p>
        </div>

        <div class="image-wrap">
          <img src="/customer-assets/auth-event-parking.jpg" alt="Event parking reservation experience">
        </div>

        <div class="benefits">
          <article><span>◫</span><div><b>Find Events</b><small>Discover experiences near you</small></div></article>
          <article><span>▣</span><div><b>Book Parking</b><small>Reserve your spot in advance</small></div></article>
          <article><span>◎</span><div><b>Enjoy More</b><small>Less stress. More experiences.</small></div></article>
        </div>
      </div>

      <div class="form-side">
        <div class="top-link">New here? <a routerLink="/register">Create an account</a></div>

        <form class="login-card" (ngSubmit)="go()">
          <a routerLink="/" class="form-logo"><img src="/customer-assets/eventpark-logo.svg" alt="EventPark"></a>
          <span class="kicker">CUSTOMER ACCESS</span>
          <h1>Sign in to EventPark</h1>
          <p>Access your account to manage bookings, view upcoming events and continue your EventPark journey.</p>

          <label>Email address
            <div class="field"><span>✉</span><input [(ngModel)]="email" name="email" type="email" autocomplete="email" placeholder="you@yourmail.com"></div>
          </label>

          <label>Password
            <div class="field"><span>▣</span><input [(ngModel)]="password" name="password" type="password" autocomplete="current-password" placeholder="Enter your password"></div>
          </label>

          @if(error()){<div class="error">{{error()}}</div>}

          <button class="sign-btn">Sign in <span>→</span></button>

          <div class="divider"><i></i><span>EVENTPARK CUSTOMER</span><i></i></div>
          <p class="foot">Don't have an account? <a routerLink="/register">Create an account →</a></p>
        </form>
      </div>
    </section>
  `,
  styles:[`
    :host{display:block;--navy:#06393d;--teal:#0b927d;--mint:#e8f7f3;--ink:#17373d;--muted:#6f8589}
    .auth-shell{min-height:100vh;display:grid;grid-template-columns:1.08fr .92fr;background:linear-gradient(135deg,#edf9f6,#ffffff 62%,#eaf8f5)}
    .visual-panel{position:relative;overflow:hidden;padding:34px 44px;color:#fff;background:linear-gradient(145deg,#07383af2,#086458e8),url('/customer-assets/customer-operations-bg.png') center/cover;display:flex;flex-direction:column}
    .visual-panel:after{content:"";position:absolute;width:520px;height:520px;border-radius:50%;right:-250px;bottom:-240px;border:1px solid #ffffff25}
    .brand img{width:165px;display:block}
    .visual-copy{margin:34px 0 20px;position:relative;z-index:1}
    .eyebrow,.kicker{font-size:11px;letter-spacing:1.7px;font-weight:950;color:#f0b52f}
    .visual-copy h2{font-size:54px;line-height:.98;letter-spacing:-2.5px;margin:10px 0;color:#fff}
    .visual-copy h2 em{font-style:normal;color:#54e3c5}
    .visual-copy p{font-size:18px;color:#d4e8e4;margin:0}
    .image-wrap{position:relative;z-index:1;border-radius:20px;overflow:hidden;border:1px solid #ffffff25;box-shadow:0 22px 48px #04282d60}
    .image-wrap img{width:100%;height:330px;object-fit:cover;display:block}
    .benefits{position:relative;z-index:1;display:grid;grid-template-columns:repeat(3,1fr);gap:10px;margin-top:15px}
    .benefits article{padding:13px;border-radius:13px;background:#ffffff0f;border:1px solid #ffffff1e;display:flex;gap:9px;align-items:flex-start}
    .benefits article>span{width:34px;height:34px;border-radius:50%;display:grid;place-items:center;background:#0fa38b;color:#fff;font-weight:950}
    .benefits div{display:flex;flex-direction:column;gap:3px}.benefits b{font-size:13px}.benefits small{font-size:10px;color:#bfd6d2;line-height:1.4}

    .form-side{position:relative;display:grid;place-items:center;padding:70px 34px 34px;background:linear-gradient(145deg,#fffffff6,#effaf7f0)}
    .form-side:before{content:"";position:absolute;inset:0;background:url('/customer-assets/customer-operations-bg.png') center/cover;opacity:.18;pointer-events:none}
    .top-link{position:absolute;right:34px;top:24px;z-index:1;color:#6f8589;font-size:13px}.top-link a{margin-left:8px;padding:10px 14px;border:1px solid #83cbbd;border-radius:999px;color:#0b7f6e;text-decoration:none;font-weight:900;background:#fff}
    .login-card{position:relative;z-index:1;width:min(500px,100%);padding:34px;background:rgba(255,255,255,.91);border:1px solid #9bd4c9;border-radius:24px;box-shadow:0 28px 65px #07383a18;backdrop-filter:blur(16px);display:flex;flex-direction:column;gap:15px}
    .form-logo{display:flex;justify-content:center}.form-logo img{width:170px}
    .login-card h1{font-size:38px;letter-spacing:-1.4px;text-align:center;margin:2px 0;color:var(--navy)}
    .login-card>p{font-size:15px;line-height:1.6;text-align:center;color:var(--muted);margin:0 0 4px}
    .kicker{text-align:center;color:#0b927d}
    .login-card label{display:flex;flex-direction:column;gap:7px;font-size:13px;font-weight:850;color:#35555b}
    .field{display:flex;align-items:center;gap:10px;min-height:50px;padding:0 12px;border:1px solid #b4dcd5;border-radius:12px;background:#fff}
    .field span{color:#70898e}.field input{flex:1;border:0;outline:0;font-size:14px;background:transparent;color:#17373d}
    .sign-btn{min-height:50px;border:0;border-radius:14px;background:linear-gradient(135deg,#06796d,#0ba187);color:#fff;font-size:15px;font-weight:950;cursor:pointer;display:flex;align-items:center;justify-content:center;gap:12px;box-shadow:0 13px 27px #0b927d25}
    .sign-btn span{font-size:20px}.sign-btn:hover{filter:brightness(1.05)}
    .error{padding:11px 12px;border-radius:10px;background:#fff0f0;border:1px solid #efb4b4;color:#b42626;font-size:12px}
    .divider{display:flex;align-items:center;gap:10px;color:#94a4a7;font-size:9px;font-weight:900;letter-spacing:1px}.divider i{height:1px;background:#d9e7e4;flex:1}
    .foot{text-align:center!important;font-size:13px!important}.foot a{color:#0b927d;text-decoration:none;font-weight:950}
    @media(max-width:900px){.auth-shell{grid-template-columns:1fr}.visual-panel{display:none}.form-side{min-height:100vh;padding:80px 20px 30px}.top-link{right:20px}.login-card{padding:27px}}
  `]
})
export class LoginComponent{
  private a=inject(AuthService);
  email='';password='';readonly error=signal('');
  go(){this.error.set('');this.a.login({email:this.email,password:this.password}).subscribe({next:()=>this.a.goAfterLogin(),error:e=>this.error.set(e?.error?.message??'Login failed')})}
}
