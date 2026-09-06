import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { API_BASE_URL } from '../config/api.config';
@Injectable({providedIn:'root'})
export class ApiService{
 private http=inject(HttpClient);
 get<T>(p:string,q:Record<string,any>={}){let x=new HttpParams();Object.entries(q).forEach(([k,v])=>{if(v!==null&&v!==undefined&&v!=='')x=x.set(k,String(v));});return this.http.get<T>(API_BASE_URL+p,{params:x});}
 post<T>(p:string,b:any={}){return this.http.post<T>(API_BASE_URL+p,b)}
 put<T>(p:string,b:any={}){return this.http.put<T>(API_BASE_URL+p,b)}
 delete<T>(p:string){return this.http.delete<T>(API_BASE_URL+p)}
 blob(p:string){return this.http.get(API_BASE_URL+p,{responseType:'blob'})}
}
