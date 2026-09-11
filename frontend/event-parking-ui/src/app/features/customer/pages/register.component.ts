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
          <span class="eyebrow">CREATE YOUR EVENTPARK ACCOUNT</span>
          <h2>Your event<br>journey starts<br><em>here.</em></h2>
          <p>Create one customer account for events, tickets, parking and booking updates.</p>
        </div>

        <div class="image-wrap">
          <img src="/customer-assets/auth-event-parking-premium.jpg" alt="Event venue parking">
          <div class="image-note"><small>EVENTPARK CUSTOMER</small><b>Book smart. Park easy. Enjoy more.</b></div>
        </div>

        <div class="benefits">
          <article><span>01</span><div><b>Discover</b><small>Find your event</small></div></article>
          <article><span>02</span><div><b>Reserve</b><small>Seat + parking</small></div></article>
          <article><span>03</span><div><b>Attend</b><small>Use your QR</small></div></article>
        </div>
      </div>

      <div class="form-side">
        <a routerLink="/" class="back-home">← Back to home</a>

        <form class="card" (ngSubmit)="go()">
          <a routerLink="/" class="form-logo"><img src="/customer-assets/eventpark-logo.svg" alt="EventPark"></a>
          <span class="kicker">CUSTOMER REGISTRATION</span>
          <h1>Create your account</h1>
          <p>Join EventPark and keep your complete event experience in one place.</p>

          <label>Full name<div class="field"><span>◎</span><input [(ngModel)]="name" name="n" autocomplete="name" placeholder="Your full name"></div></label>
          <label>Email address<div class="field"><span>✉</span><input [(ngModel)]="email" name="e" type="email" autocomplete="email" placeholder="you@example.com"></div></label>
          <label>Phone number<div class="field"><span>☎</span><input [(ngModel)]="phone" name="p" autocomplete="tel" placeholder="+94 77 123 4567"></div></label>
          <label>Password<div class="field"><span>▣</span><input [(ngModel)]="password" name="pw" type="password" autocomplete="new-password" placeholder="Minimum 8 characters"></div></label>

          @if(error()){<div class="error">{{error()}}</div>}

          <button [disabled]="loading()">{{loading() ? 'Creating account...' : 'Create customer account'}} <span>→</span></button>

          <div class="divider"><i></i><span>ALREADY REGISTERED?</span><i></i></div>
          <p class="foot"><a routerLink="/login/customer">Customer sign in →</a></p>
        </form>
      </div>
    </section>
  `,
  styles:[`
    :host{display:block;--navy:#062f3c;--blue:#1677ff;--teal:#0b927d;--muted:#6f8589}.auth-shell{min-height:100vh;display:grid;grid-template-columns:1fr 1fr;background:linear-gradient(135deg,#eaf4ff,#f8fbff 50%,#edf9f6)}
    .visual-panel{position:relative;overflow:hidden;padding:34px 44px;color:#fff;background:linear-gradient(145deg,rgba(3,31,55,.97),rgba(5,54,83,.92) 50%,rgba(7,92,94,.90)),url('/customer-assets/customer-operations-bg.png') center/cover;display:flex;flex-direction:column}.brand img{width:165px}
    .visual-copy{margin:26px 0 17px}.eyebrow,.kicker{font-size:10px;letter-spacing:1.8px;font-weight:950;color:#65c9ff}.visual-copy h2{font-size:54px;line-height:.94;letter-spacing:-2.5px;margin:10px 0;color:#fff}.visual-copy h2 em{font-style:normal;color:#58dfc3}.visual-copy p{font-size:15px;line-height:1.65;color:#c5dbe4;max-width:540px}
    .image-wrap{position:relative;border-radius:21px;overflow:hidden;border:1px solid #8bd4ff33;box-shadow:0 24px 50px #01172462}.image-wrap img{width:100%;height:300px;object-fit:cover;display:block}.image-wrap:after{content:"";position:absolute;inset:0;background:linear-gradient(180deg,transparent 45%,rgba(2,26,44,.68))}.image-note{position:absolute;z-index:2;left:15px;bottom:15px;padding:10px 12px;border-radius:11px;background:#062f3de0;display:flex;flex-direction:column}.image-note small{font-size:7px;color:#67caff;font-weight:950}.image-note b{font-size:11px}
    .benefits{display:grid;grid-template-columns:repeat(3,1fr);gap:9px;margin-top:12px}.benefits article{padding:11px;border-radius:12px;background:#ffffff0c;border:1px solid #77d0ff20;display:flex;gap:8px;align-items:center}.benefits article>span{width:32px;height:32px;border-radius:9px;background:linear-gradient(135deg,#1677ff,#0b927d);display:grid;place-items:center;font-size:8px;font-weight:950}.benefits div{display:flex;flex-direction:column}.benefits b{font-size:11px}.benefits small{font-size:8px;color:#b9d0d9}
    .form-side{position:relative;display:grid;place-items:center;padding:58px 34px 30px;background:linear-gradient(145deg,#fff,#f4faff)}.back-home{position:absolute;right:28px;top:22px;color:#5d7883;text-decoration:none;font-size:11px;font-weight:850}.card{width:min(510px,100%);padding:29px 32px;background:#fffffff5;border:1px solid #b9d6e5;border-radius:24px;box-shadow:0 30px 70px #0a46711a;display:flex;flex-direction:column;gap:11px}.form-logo{display:flex;justify-content:center}.form-logo img{width:170px}.kicker{text-align:center;color:#1677ff}.card h1{text-align:center;font-size:35px;letter-spacing:-1.3px;margin:0;color:var(--navy)}.card>p{text-align:center;color:var(--muted);font-size:13px;line-height:1.6;margin:0 0 2px}
    .card label{display:flex;flex-direction:column;gap:5px;font-size:11px;font-weight:850;color:#35555b}.field{display:flex;align-items:center;gap:9px;min-height:45px;padding:0 11px;border:1px solid #bdd7e5;border-radius:11px;background:#fff}.field:focus-within{border-color:#4e9fe2;box-shadow:0 0 0 3px #1677ff12}.field span{color:#66838f}.field input{flex:1;border:0;outline:0;font-size:13px;background:transparent}
    .card button{min-height:48px;border:0;border-radius:13px;background:linear-gradient(135deg,#0b66dc,#1677ff 62%,#0b927d);color:#fff;font-size:13px;font-weight:950;display:flex;align-items:center;justify-content:center;gap:10px;cursor:pointer}.card button:disabled{opacity:.6}.error{padding:10px;border:1px solid #efb4b4;background:#fff0f0;color:#b42626;border-radius:9px;font-size:10px}.divider{display:flex;align-items:center;gap:9px;color:#93a4a8;font-size:8px;font-weight:900}.divider i{height:1px;background:#d8e6e4;flex:1}.foot{text-align:center!important;margin:0!important}.foot a{color:#0a66c7;text-decoration:none;font-size:11px;font-weight:950}
    @media(max-width:900px){.auth-shell{grid-template-columns:1fr}.visual-panel{display:none}.form-side{min-height:100vh;padding:58px 18px 22px}.card{padding:25px}.back-home{right:18px}}
  `]
})
export class RegisterComponent{
  private auth=inject(AuthService);
  private router=inject(Router);
  name='';email='';phone='';password='';
  readonly error=signal('');
  readonly loading=signal(false);

  go(){
    this.error.set('');
    if(!this.name.trim()||!this.email.trim()||this.password.length<8){
      this.error.set('Enter your name, a valid email and a password with at least 8 characters.');
      return;
    }
    this.loading.set(true);
    this.auth.register({fullName:this.name.trim(),email:this.email.trim(),phone:this.phone.trim(),password:this.password}).subscribe({
      next:()=>this.router.navigateByUrl('/login/customer'),
      error:e=>{this.loading.set(false);this.error.set(e?.error?.message??'Registration failed')}
    })
  }
}
