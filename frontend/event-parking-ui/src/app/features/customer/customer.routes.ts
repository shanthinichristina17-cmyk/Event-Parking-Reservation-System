import { Routes } from '@angular/router';
import { customerGuard } from '../../core/guards/customer.guard';

export const CUSTOMER_ROUTES:Routes=[
 {path:'login',loadComponent:()=>import('./pages/login.component').then(m=>m.LoginComponent),data:{loginRole:'choose'}},
 {path:'login/customer',loadComponent:()=>import('./pages/login.component').then(m=>m.LoginComponent),data:{loginRole:'customer'}},
 {path:'login/admin',loadComponent:()=>import('./pages/login.component').then(m=>m.LoginComponent),data:{loginRole:'admin'}},
 {path:'register',loadComponent:()=>import('./pages/register.component').then(m=>m.RegisterComponent)},
 {path:'forgot-password',loadComponent:()=>import('./pages/forgot-password.component').then(m=>m.ForgotPasswordComponent)},
 {path:'reset-password',loadComponent:()=>import('./pages/reset-password.component').then(m=>m.ResetPasswordComponent)},
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
