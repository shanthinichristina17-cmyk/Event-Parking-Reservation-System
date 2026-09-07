import { Component,inject,signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector:'app-login',
  imports:[FormsModule,RouterLink],
  template:`
    <section class="auth">
      <div class="side">
        <a routerLink="/"><img class="logo" src="/customer-assets/eventpark-logo-light.svg" alt="EventPark"></a>
        <div><span>CONNECTED BOOKING EXPERIENCE</span><h2>Your event journey,<br>all in one place.</h2><p>Browse events, reserve seats, add parking and keep your QR tickets together.</p></div>
        <img class="art" src="/customer-assets/auth-event-parking.jpg" alt="Event parking reservation experience" (error)="artFallback($event)">
      </div>

      <div class="form-wrap">
        <form class="form" (ngSubmit)="go()">
          <a routerLink="/" class="mobile-logo"><img src="/customer-assets/eventpark-logo.svg" alt="EventPark"></a>
          <span class="kicker">WELCOME BACK</span><h1>Sign in to EventPark</h1><p>Continue to your bookings and upcoming events.</p>
          <label>Email address<input [(ngModel)]="email" name="email" type="email" autocomplete="email" placeholder="you@example.com"></label>
          <label>Password<input [(ngModel)]="password" name="password" type="password" autocomplete="current-password" placeholder="Your password"></label>
          @if(error()){<div class="error">{{error()}}</div>}
          <button>Sign in →</button>
          <p class="foot">New to EventPark? <a routerLink="/register">Create an account</a></p>
        </form>
      </div>
    </section>
  `,
  styles:[`
    :host{--ink:#07363a;--teal:#0b7a69;--muted:#718184;display:block}.auth{min-height:100vh;display:grid;grid-template-columns:1fr 1fr;background:#f7f7f2}.side{padding:42px 52px;background:linear-gradient(145deg,#07363a 0%,#07544d 52%,#0b7a69 100%);color:#fff;display:flex;flex-direction:column;justify-content:space-between;overflow:hidden}.logo{width:160px;display:block}.side span,.kicker{font-size:9px;letter-spacing:2px;font-weight:900;color:#f6bf37}.side h2{font-size:46px;line-height:1.02;letter-spacing:-2px;margin:10px 0}.side p{max-width:500px;color:#c6dbd7;line-height:1.7}.art{width:min(690px,100%);margin:20px auto 0;border-radius:22px;border:1px solid #ffffff24;box-shadow:0 24px 48px #031f242e;display:block}
    .form-wrap{display:grid;place-items:center;padding:30px}.form{width:min(440px,100%);background:#fff;padding:34px;border:1px solid #dce7e4;border-radius:20px;box-shadow:0 20px 55px #07363a10;display:flex;flex-direction:column;gap:14px}.form h1{font-size:34px;letter-spacing:-1px;margin:0;color:var(--ink)}.form>p{color:var(--muted);margin:0 0 6px}.form label{display:flex;flex-direction:column;gap:7px;font-size:11px;font-weight:800;color:#33494c}.form input{min-height:45px;border:1px solid #d8e4e1;border-radius:11px;padding:0 12px;outline:0}.form input:focus{border-color:var(--teal);box-shadow:0 0 0 3px #0b7a6916}.form button{min-height:46px;border:0;border-radius:11px;background:var(--ink);color:#fff;font-weight:900;cursor:pointer}.form button:hover{background:var(--teal)}.error{padding:10px;border-radius:10px;background:#fff1eb;color:#c2410c;font-size:11px}.foot{text-align:center!important;font-size:11px}.foot a{color:var(--teal);font-weight:900}.mobile-logo{display:none}.mobile-logo img{width:150px}
    @media(max-width:760px){.auth{grid-template-columns:1fr}.side{display:none}.mobile-logo{display:block;margin-bottom:8px}.form-wrap{padding:20px}.form{padding:26px}}
  `]
})
export class LoginComponent{
  private a=inject(AuthService);email='';password='';readonly error=signal('');
  go(){this.a.login({email:this.email,password:this.password}).subscribe({next:()=>this.a.goAfterLogin(),error:e=>this.error.set(e?.error?.message??'Login failed')})}
  artFallback(ev:Event){const img=ev.target as HTMLImageElement;if(!img.src.endsWith('/customer-assets/hero-platform.svg'))img.src='/customer-assets/hero-platform.svg'}
}
