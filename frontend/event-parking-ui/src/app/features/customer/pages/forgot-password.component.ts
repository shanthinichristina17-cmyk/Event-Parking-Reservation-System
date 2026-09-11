import { Component,inject,signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute,RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector:'app-forgot-password',
  imports:[FormsModule,RouterLink],
  template:`
    <section class="auth-shell">
      <div class="visual-panel">
        <a routerLink="/" class="brand"><img src="/customer-assets/eventpark-logo-light.svg" alt="EventPark"></a>
        <div class="visual-copy">
          <span class="eyebrow">SECURE ACCOUNT RECOVERY</span>
          <h2>Recover access.<br><em>Return to EventPark.</em></h2>
          <p>Request a secure password reset link and continue with your EventPark account.</p>
        </div>
        <div class="image-wrap">
          <img src="/customer-assets/auth-event-parking-premium.jpg" alt="Event venue parking">
          <div class="image-note"><small>SECURE RESET</small><b>Your reset link expires after 45 minutes.</b></div>
        </div>
      </div>

      <div class="form-side">
        <form class="card" (ngSubmit)="send()">
          <a [routerLink]="backLink()" class="back">← Back to {{role()==='admin' ? 'Admin' : 'Customer'}} sign in</a>
          <a routerLink="/" class="form-logo"><img src="/customer-assets/eventpark-logo.svg" alt="EventPark"></a>

          @if(!sent()){
            <span class="kicker">{{role()==='admin' ? 'ADMIN RECOVERY' : 'CUSTOMER RECOVERY'}}</span>
            <h1>Forgot your password?</h1>
            <p>Enter the email connected to your EventPark account. If the account exists, we'll send a secure reset link.</p>

            <label>Email address
              <div class="field"><span>✉</span><input [(ngModel)]="email" name="email" type="email" autocomplete="email" placeholder="you@example.com"></div>
            </label>

            @if(error()){<div class="error">{{error()}}</div>}
            <button [disabled]="loading()">{{loading() ? 'Requesting...' : 'Send reset link'}} <span>→</span></button>
          } @else {
            <div class="success">✓</div>
            <span class="kicker">CHECK YOUR EMAIL</span>
            <h1>Reset link requested</h1>
            <p>{{message()}}</p>
            <a [routerLink]="backLink()" class="primary-link">Back to sign in →</a>
            <button class="secondary" type="button" (click)="sent.set(false)">Try another email</button>
          }
        </form>
      </div>
    </section>
  `,
  styles:[`
    :host{display:block;--navy:#062f3c;--blue:#1677ff;--teal:#0b927d;--muted:#6f8589}.auth-shell{min-height:100vh;display:grid;grid-template-columns:1fr 1fr;background:linear-gradient(135deg,#eaf4ff,#f8fbff 50%,#edf9f6)}
    .visual-panel{padding:38px 46px;color:#fff;background:linear-gradient(145deg,rgba(3,31,55,.97),rgba(5,54,83,.92) 50%,rgba(7,92,94,.90)),url('/customer-assets/customer-operations-bg.png') center/cover;display:flex;flex-direction:column}.brand img{width:165px}.visual-copy{margin:52px 0 24px}.eyebrow,.kicker{font-size:10px;letter-spacing:1.8px;font-weight:950;color:#65c9ff}.visual-copy h2{font-size:50px;line-height:.98;letter-spacing:-2.2px;margin:10px 0}.visual-copy h2 em{font-style:normal;color:#58dfc3}.visual-copy p{font-size:15px;line-height:1.65;color:#c5dbe4;max-width:540px}
    .image-wrap{position:relative;border-radius:21px;overflow:hidden;border:1px solid #8bd4ff33;box-shadow:0 24px 50px #01172462}.image-wrap img{width:100%;height:320px;object-fit:cover;display:block}.image-wrap:after{content:"";position:absolute;inset:0;background:linear-gradient(180deg,transparent 48%,rgba(2,26,44,.68))}.image-note{position:absolute;z-index:2;left:15px;bottom:15px;padding:10px 12px;border-radius:11px;background:#062f3de0;display:flex;flex-direction:column}.image-note small{font-size:7px;color:#67caff;font-weight:950}.image-note b{font-size:11px}
    .form-side{display:grid;place-items:center;padding:34px;background:linear-gradient(145deg,#fff,#f4faff)}.card{width:min(500px,100%);padding:32px;background:#fffffff5;border:1px solid #b9d6e5;border-radius:24px;box-shadow:0 30px 70px #0a46711a;display:flex;flex-direction:column;gap:14px}.back{color:#0a66c7;text-decoration:none;font-size:10px;font-weight:900}.form-logo{display:flex;justify-content:center}.form-logo img{width:170px}.kicker{text-align:center;color:#1677ff}.card h1{text-align:center;font-size:35px;letter-spacing:-1.3px;margin:0;color:var(--navy)}.card>p{text-align:center;color:var(--muted);font-size:13px;line-height:1.6;margin:0}.card label{display:flex;flex-direction:column;gap:6px;font-size:11px;font-weight:850;color:#35555b}.field{display:flex;align-items:center;gap:9px;min-height:48px;padding:0 11px;border:1px solid #bdd7e5;border-radius:11px;background:#fff}.field input{flex:1;border:0;outline:0;font-size:13px}.card button,.primary-link{min-height:48px;border:0;border-radius:13px;background:linear-gradient(135deg,#0b66dc,#1677ff 62%,#0b927d);color:#fff;font-size:12px;font-weight:950;display:flex;align-items:center;justify-content:center;gap:10px;text-decoration:none;cursor:pointer}.secondary{background:#fff!important;color:#0a66c7!important;border:1px solid #b6d1e4!important}.error{padding:10px;border:1px solid #efb4b4;background:#fff0f0;color:#b42626;border-radius:9px;font-size:10px}.success{width:62px;height:62px;border-radius:50%;background:#e4f9ed;color:#168143;display:grid;place-items:center;margin:0 auto;font-size:27px;font-weight:950}
    @media(max-width:900px){.auth-shell{grid-template-columns:1fr}.visual-panel{display:none}.form-side{min-height:100vh;padding:20px}.card{padding:26px}}
  `]
})
export class ForgotPasswordComponent{
  private auth=inject(AuthService);
  private route=inject(ActivatedRoute);

  readonly role=signal<'customer'|'admin'>(this.route.snapshot.queryParamMap.get('role')==='admin'?'admin':'customer');
  readonly sent=signal(false);
  readonly loading=signal(false);
  readonly error=signal('');
  readonly message=signal('If the account exists, a password reset email was sent.');
  email='';

  backLink(){return this.role()==='admin'?'/login/admin':'/login/customer'}

  send(){
    this.error.set('');
    if(!this.email.trim()){this.error.set('Enter your account email.');return}
    this.loading.set(true);
    this.auth.forgotPassword(this.email.trim()).subscribe({
      next:(r:any)=>{this.loading.set(false);this.message.set(r?.message||'If the account exists, a password reset email was sent.');this.sent.set(true)},
      error:e=>{this.loading.set(false);this.error.set(e?.error?.message||'Could not request a reset link.')}
    })
  }
}
