import { Component,inject,signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router,RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector:'app-register',
  imports:[FormsModule,RouterLink],
  template:`
    <section class="auth-shell">
      <div class="visual-panel">
        <a routerLink="/" class="brand"><img src="/customer-assets/eventpark-logo-light.svg" alt="EventPark"></a>
        <div class="visual-copy">
          <span class="eyebrow">PARK • ATTEND • ENJOY</span>
          <h2>More<br>Experiences<br><em>Ahead.</em></h2>
          <p>Create your EventPark account and unlock a smoother journey from discovery to parking and ticket confirmation.</p>
        </div>
        <div class="image-wrap"><img src="/customer-assets/auth-event-parking.jpg" alt="Event parking and venue"></div>
        <div class="benefits">
          <article><span>▣</span><div><b>Find Parking</b><small>Reserve your spot in seconds</small></div></article>
          <article><span>◫</span><div><b>Attend Events</b><small>Concerts, sports and more</small></div></article>
          <article><span>♥</span><div><b>Enjoy the Moment</b><small>Less stress. More memories.</small></div></article>
        </div>
      </div>

      <div class="form-side">
        <form class="register-card" (ngSubmit)="go()">
          <a routerLink="/" class="form-logo"><img src="/customer-assets/eventpark-logo.svg" alt="EventPark"></a>
          <span class="kicker">CREATE CUSTOMER ACCOUNT</span>
          <h1>Create your account</h1>
          <p>Join EventPark and get closer to the events you love.</p>

          <label>Full name<div class="field"><span>◎</span><input [(ngModel)]="name" name="n" autocomplete="name" placeholder="e.g. Alex Perera"></div></label>
          <label>Email address<div class="field"><span>✉</span><input [(ngModel)]="email" name="e" type="email" autocomplete="email" placeholder="alex@example.com"></div></label>
          <label>Phone number<div class="field"><span>☎</span><input [(ngModel)]="phone" name="p" autocomplete="tel" placeholder="+94 77 123 4567"></div></label>
          <label>Password<div class="field"><span>▣</span><input [(ngModel)]="password" name="pw" type="password" autocomplete="new-password" placeholder="Create a strong password"></div></label>

          @if(error()){<div class="error">{{error()}}</div>}

          <button>Create account <span>→</span></button>
          <div class="divider"><i></i><span>Already have an account?</span><i></i></div>
          <p class="foot"><a routerLink="/login">Sign in →</a></p>
        </form>
      </div>
    </section>
  `,
  styles:[`
    :host{display:block;--navy:#06393d;--teal:#0b927d;--ink:#17373d;--muted:#6f8589}
    .auth-shell{min-height:100vh;display:grid;grid-template-columns:1.05fr .95fr;background:linear-gradient(135deg,#effaf7,#fff 60%,#e7f7f2)}
    .visual-panel{position:relative;overflow:hidden;padding:34px 44px;color:#fff;background:linear-gradient(145deg,#07383af2,#07554fe8),url('/customer-assets/customer-operations-bg.png') center/cover;display:flex;flex-direction:column}
    .brand img{width:165px}.visual-copy{margin:28px 0 16px}.eyebrow,.kicker{font-size:11px;letter-spacing:1.7px;font-weight:950;color:#f0b52f}
    .visual-copy h2{font-size:54px;line-height:.98;letter-spacing:-2.4px;margin:9px 0;color:#fff}.visual-copy h2 em{font-style:normal;color:#56e4c6}
    .visual-copy p{font-size:16px;line-height:1.7;color:#d0e4e0;max-width:580px}
    .image-wrap{border-radius:19px;overflow:hidden;border:1px solid #ffffff25;box-shadow:0 21px 45px #04282d55}.image-wrap img{width:100%;height:300px;object-fit:cover;display:block}
    .benefits{display:grid;grid-template-columns:repeat(3,1fr);gap:9px;margin-top:14px}.benefits article{padding:12px;border-radius:12px;background:#ffffff0f;border:1px solid #ffffff1f;display:flex;gap:8px}.benefits article>span{width:32px;height:32px;border-radius:50%;background:#0ea28a;display:grid;place-items:center;color:#fff;font-weight:950}.benefits div{display:flex;flex-direction:column}.benefits b{font-size:12px}.benefits small{font-size:9px;color:#bed5d0;line-height:1.4}
    .form-side{position:relative;display:grid;place-items:center;padding:34px;background:linear-gradient(145deg,#fffffff7,#effaf7f2)}.form-side:before{content:"";position:absolute;inset:0;background:url('/customer-assets/customer-operations-bg.png') center/cover;opacity:.17;pointer-events:none}
    .register-card{position:relative;z-index:1;width:min(510px,100%);padding:30px 34px;background:rgba(255,255,255,.92);border:1px solid #9bd4c9;border-radius:24px;box-shadow:0 28px 65px #07383a18;backdrop-filter:blur(16px);display:flex;flex-direction:column;gap:11px}
    .form-logo{display:flex;justify-content:center}.form-logo img{width:170px}.kicker{text-align:center;color:#0b927d}.register-card h1{font-size:36px;letter-spacing:-1.3px;text-align:center;margin:0;color:var(--navy)}.register-card>p{text-align:center;margin:0 0 3px;color:var(--muted);font-size:14px}
    .register-card label{display:flex;flex-direction:column;gap:5px;font-size:12px;font-weight:850;color:#38575d}.field{display:flex;align-items:center;gap:10px;min-height:46px;padding:0 12px;border:1px solid #b4dcd5;border-radius:11px;background:#fff}.field span{color:#70898e}.field input{flex:1;border:0;outline:0;background:transparent;font-size:14px;color:#17373d}
    .register-card button{min-height:48px;border:0;border-radius:14px;background:linear-gradient(135deg,#06796d,#0ba187);color:#fff;font-size:14px;font-weight:950;cursor:pointer;display:flex;align-items:center;justify-content:center;gap:12px;box-shadow:0 13px 27px #0b927d25}.register-card button span{font-size:19px}.error{padding:10px 11px;border-radius:9px;background:#fff0f0;border:1px solid #efb4b4;color:#b42626;font-size:11px}
    .divider{display:flex;align-items:center;gap:10px;color:#889b9f;font-size:9px}.divider i{height:1px;background:#d9e7e4;flex:1}.foot{text-align:center!important;margin:0!important}.foot a{color:#0b927d;text-decoration:none;font-size:13px;font-weight:950}
    @media(max-width:900px){.auth-shell{grid-template-columns:1fr}.visual-panel{display:none}.form-side{min-height:100vh;padding:25px 20px}.register-card{padding:26px}}
  `]
})
export class RegisterComponent{
  private a=inject(AuthService);private r=inject(Router);
  name='';email='';phone='';password='';readonly error=signal('');
  go(){this.error.set('');this.a.register({fullName:this.name,email:this.email,phone:this.phone,password:this.password}).subscribe({next:()=>this.r.navigateByUrl('/login'),error:e=>this.error.set(e?.error?.message??'Registration failed')})}
}
