import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const customerGuard:CanActivateFn=()=>{
  const a=inject(AuthService);
  return a.isCustomer()?true:inject(Router).createUrlTree(['/login/customer']);
};
