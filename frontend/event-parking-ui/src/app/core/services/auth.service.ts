import { computed, inject, Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { tap } from 'rxjs';
import { ApiService } from './api.service';
@Injectable({providedIn:'root'})
export class AuthService{
 private api=inject(ApiService);private router=inject(Router);private state=signal<any>(this.read());
 readonly user=this.state.asReadonly();readonly isLoggedIn=computed(()=>!!localStorage.getItem('eventpark_token'));readonly isAdmin=computed(()=>this.state()?.role==='Admin');readonly isCustomer=computed(()=>this.state()?.role==='Customer');
 login(body:any){return this.api.post<any>('/auth/login',body).pipe(tap(r=>{localStorage.setItem('eventpark_token',r.token);localStorage.setItem('eventpark_user',JSON.stringify(r.customer));this.state.set(r.customer)}));}
 register(body:any){return this.api.post('/auth/register',body)} token(){return localStorage.getItem('eventpark_token')}
 logout(){localStorage.clear();this.state.set(null);this.router.navigateByUrl('/login')}
 goAfterLogin(){this.router.navigateByUrl(this.isAdmin()?'/admin/dashboard':'/dashboard')}
 private read(){try{return JSON.parse(localStorage.getItem('eventpark_user')||'null')}catch{return null}}
}
