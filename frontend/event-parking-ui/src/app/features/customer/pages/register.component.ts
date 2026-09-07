import { Component,inject,signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router,RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector:'app-register',
  imports:[FormsModule,RouterLink],
  template:`
    <section class="auth">
      <div class="side">
        <a routerLink="/"><img class="logo" src="/customer-assets/eventpark-logo-light.svg" alt="EventPark"></a>
        <div class="side-content"><span>JOIN EVENTPARK</span><h2>One account.<br>Every experience.</h2><p>Keep your event tickets, parking choices and booking notifications together.</p><div class="brand-art"><img src="/customer-assets/eventpark-brand-full.png" alt="EventPark — Park Smart, Enjoy More" (error)="brandFallback($event)"></div></div>
        <div class="mini-grid"><div><b>✓</b><span>Exact seat selection</span></div><div><b>✓</b><span>Optional parking</span></div><div><b>✓</b><span>QR tickets</span></div></div>
      </div>

      <div class="form-wrap">
        <form class="form" (ngSubmit)="go()">
          <a routerLink="/" class="mobile-logo"><img src="/customer-assets/eventpark-logo.svg" alt="EventPark"></a>
          <span class="kicker">CREATE ACCOUNT</span><h1>Start booking smarter</h1><p>Register once and use the complete customer booking flow.</p>
          <label>Full name<input [(ngModel)]="name" name="n" autocomplete="name" placeholder="Your full name"></label>
          <label>Email address<input [(ngModel)]="email" name="e" type="email" autocomplete="email" placeholder="you@example.com"></label>
          <label>Phone<input [(ngModel)]="phone" name="p" autocomplete="tel" placeholder="0771234567"></label>
          <label>Password<input [(ngModel)]="password" name="pw" type="password" autocomplete="new-password" placeholder="Minimum 8 characters"></label>
          @if(error()){<div class="error">{{error()}}</div>}
          <button>Create account →</button>
          <p class="foot">Already registered? <a routerLink="/login">Sign in</a></p>
        </form>
      </div>
    </section>
  `,
  styles:[`
    :host{--ink:#07363a;--teal:#0b7a69;--muted:#718184;display:block}.auth{min-height:100vh;display:grid;grid-template-columns:1fr 1fr;background:#f7f7f2}.side{padding:42px 52px;background:linear-gradient(145deg,#07363a 0%,#07544d 52%,#0b7a69 100%);color:#fff;display:flex;flex-direction:column;justify-content:space-between}.logo{width:160px;display:block}.side>div>span,.kicker{font-size:9px;letter-spacing:2px;font-weight:900;color:#f6bf37}.side h2{font-size:48px;line-height:1.02;letter-spacing:-2px;margin:10px 0}.side p{max-width:500px;color:#c6dbd7;line-height:1.7}.brand-art{margin-top:24px;width:min(430px,88%);padding:18px 22px;background:linear-gradient(145deg,#ffffff 0%,#f4fbf8 100%);border:1px solid #ffffff55;border-radius:24px;box-shadow:0 22px 50px #022b2d38}.brand-art img{display:block;width:100%;height:auto;max-height:285px;object-fit:contain}.mini-grid{display:grid;grid-template-columns:repeat(3,1fr);gap:9px}.mini-grid div{padding:13px;background:#ffffff10;border:1px solid #ffffff1e;border-radius:12px;display:flex;flex-direction:column;gap:6px;font-size:10px}.mini-grid b{color:#f6bf37}
    .form-wrap{display:grid;place-items:center;padding:30px}.form{width:min(460px,100%);background:#fff;padding:32px;border:1px solid #dce7e4;border-radius:20px;box-shadow:0 20px 55px #07363a10;display:flex;flex-direction:column;gap:12px}.form h1{font-size:32px;letter-spacing:-1px;margin:0;color:var(--ink)}.form>p{color:var(--muted);margin:0 0 5px}.form label{display:flex;flex-direction:column;gap:6px;font-size:11px;font-weight:800;color:#33494c}.form input{min-height:44px;border:1px solid #d8e4e1;border-radius:11px;padding:0 12px;outline:0}.form input:focus{border-color:var(--teal);box-shadow:0 0 0 3px #0b7a6916}.form button{min-height:46px;border:0;border-radius:11px;background:var(--ink);color:#fff;font-weight:900;cursor:pointer}.form button:hover{background:var(--teal)}.error{padding:10px;border-radius:10px;background:#fff1eb;color:#c2410c;font-size:11px}.foot{text-align:center!important;font-size:11px}.foot a{color:var(--teal);font-weight:900}.mobile-logo{display:none}.mobile-logo img{width:150px}
    @media(max-width:760px){.auth{grid-template-columns:1fr}.side{display:none}.mobile-logo{display:block;margin-bottom:8px}.form-wrap{padding:20px}.form{padding:26px}}
  `]
})
export class RegisterComponent{
  private a=inject(AuthService);private r=inject(Router);name='';email='';phone='';password='';readonly error=signal('');
  go(){this.a.register({fullName:this.name,email:this.email,phone:this.phone,password:this.password}).subscribe({next:()=>this.r.navigateByUrl('/login'),error:e=>this.error.set(e?.error?.message??'Registration failed')})}
  brandFallback(ev:Event){const img=ev.target as HTMLImageElement;if(!img.src.endsWith('/customer-assets/eventpark-logo.svg'))img.src='/customer-assets/eventpark-logo.svg'}
}
