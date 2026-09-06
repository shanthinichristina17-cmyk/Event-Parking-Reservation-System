import { Routes } from '@angular/router';import { customerGuard } from '../../core/guards/customer.guard';
export const CUSTOMER_ROUTES:Routes=[
 {path:'login',loadComponent:()=>import('./pages/login.component').then(m=>m.LoginComponent)},
 {path:'register',loadComponent:()=>import('./pages/register.component').then(m=>m.RegisterComponent)},
 {path:'',loadComponent:()=>import('./layout/customer-shell.component').then(m=>m.CustomerShellComponent),children:[
  {path:'',loadComponent:()=>import('./pages/home.component').then(m=>m.HomeComponent)},
  {path:'events',loadComponent:()=>import('./pages/events.component').then(m=>m.EventsComponent)},
  {path:'booking/:eventId',canActivate:[customerGuard],loadComponent:()=>import('./pages/booking-flow.component').then(m=>m.BookingFlowComponent)},
  {path:'dashboard',canActivate:[customerGuard],loadComponent:()=>import('./pages/dashboard.component').then(m=>m.DashboardComponent)},
  {path:'my-bookings',canActivate:[customerGuard],loadComponent:()=>import('./pages/my-bookings.component').then(m=>m.MyBookingsComponent)},
  {path:'notifications',canActivate:[customerGuard],loadComponent:()=>import('./pages/notifications.component').then(m=>m.NotificationsComponent)},
  {path:'profile',canActivate:[customerGuard],loadComponent:()=>import('./pages/profile.component').then(m=>m.ProfileComponent)}
 ]}
];
