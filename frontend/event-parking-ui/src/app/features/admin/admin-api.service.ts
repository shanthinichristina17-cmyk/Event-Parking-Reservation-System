import { inject, Injectable } from '@angular/core';
import { map } from 'rxjs';
import { ApiService } from '../../core/services/api.service';

@Injectable({ providedIn: 'root' })
export class AdminApiService {
  private api = inject(ApiService);

  dashboard(){return this.api.get<any>('/admin/dashboard')}

  customers(search=''){return this.api.get<any>('/customers',{search,page:1,pageSize:100})}
  deactivate(id:number){return this.api.delete<void>(`/customers/${id}`)}
  reactivate(id:number){return this.api.put<void>(`/customers/${id}/reactivate`)}

  venues(){return this.api.get<any[]>('/venues')}
  addVenue(b:any){return this.api.post<any>('/venues',b)}
  delVenue(id:number){return this.api.delete<void>(`/venues/${id}`)}

  categories(){return this.api.get<any[]>('/categories')}
  addCategory(name:string){return this.api.post<any>('/categories',{name})}
  delCategory(id:number){return this.api.delete<void>(`/categories/${id}`)}

  events(){
    return this.api.get<any>('/events',{page:1,pageSize:100}).pipe(
      map((r:any)=>Array.isArray(r)?r:(r?.items??r?.Items??r?.data??[]))
    );
  }
  addEvent(b:any){return this.api.post<any>('/events',b)}
  delEvent(id:number){return this.api.delete<void>(`/events/${id}`)}

  seats(id:number){return this.api.get<any[]>(`/events/${id}/seats`)}
  genSeats(id:number,b:any){return this.api.post<any[]>(`/events/${id}/seats/generate`,b)}
  deleteSeat(eventId:number,seatId:number){return this.api.delete<void>(`/events/${eventId}/seats/${seatId}`)}

  parking(id:number){return this.api.get<any[]>(`/events/${id}/parking-slots`)}
  genParking(id:number,b:any){return this.api.post<any[]>(`/events/${id}/parking-slots/generate`,b)}
  addParkingSlot(eventId:number,b:any){return this.api.post<any>(`/events/${eventId}/parking-slots`,b)}

  bookings(status='',search=''){return this.api.get<any[]>('/admin/bookings',{status,search})}
  payments(){return this.api.get<any[]>('/admin/payments')}

  summary(from='',to=''){return this.api.get<any>('/admin/reports/summary',{from,to})}
  revenue(from='',to=''){return this.api.get<any[]>('/admin/reports/revenue-by-event',{from,to})}
  statusReport(from='',to=''){return this.api.get<any[]>('/admin/reports/booking-status',{from,to})}

  broadcast(type:string,message:string){
    return this.api.post<any>('/admin/notifications/broadcast',{type,message})
  }

  settings(){return this.api.get<any>('/admin/settings')}
  saveSettings(b:any){return this.api.put<any>('/admin/settings',b)}

  monthly(){return this.api.get<any[]>('/admin/advanced/monthly-revenue',{months:6})}
  alerts(){return this.api.get<any[]>('/admin/pro/operations-alerts')}
  paymentHealth(){return this.api.get<any>('/admin/pro/payment-health',{days:30})}
  export(name:string){return this.api.blob(`/admin/advanced/export/${name}`)}
}
