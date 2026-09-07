import { inject, Injectable } from '@angular/core';
import { ApiService } from '../../core/services/api.service';
@Injectable({providedIn:'root'})
export class CustomerApiService{
 private api=inject(ApiService);
 events(q:any={}){return this.api.get<any>('/events',q)} event(id:number){return this.api.get<any>(`/events/${id}`)}
 seats(id:number){return this.api.get<any[]>(`/events/${id}/seats`)} parking(id:number){return this.api.get<any[]>(`/events/${id}/parking-slots`)}
 hold(eventId:number,seatIds:number[]){return this.api.post<any>('/bookings/hold',{eventId,seatIds})}
 setParking(id:number,parkingSlotId:number|null){return this.api.put<any>(`/bookings/${id}/parking`,{parkingSlotId})}
 promo(id:number,promoCode:string){return this.api.post<any>(`/bookings/${id}/promo`,{promoCode})}
 summary(id:number){return this.api.get<any>(`/bookings/${id}/summary`)}
 pay(id:number,b:any){return this.api.post<any>(`/bookings/${id}/payment/simulate`,b)}
 ticket(id:number){return this.api.get<any>(`/bookings/${id}/ticket`)} qr(id:number){return this.api.blob(`/bookings/${id}/ticket/qr`)}
 bookings(tab=''){return this.api.get<any[]>('/bookings/me',{tab})} cancel(id:number){return this.api.post<any>(`/bookings/${id}/cancel`)}
 notifications(){return this.api.get<any[]>('/notifications')} markRead(id:number){return this.api.put<void>(`/notifications/${id}/read`)}
 dashboard(){return this.api.get<any>('/customer/advanced/dashboard')} recommendations(){return this.api.get<any[]>('/customer/advanced/recommendations',{limit:6})}
 me(){return this.api.get<any>('/customers/me')} updateMe(id:number,b:any){return this.api.put<any>(`/customers/${id}`,b)}
}
