import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const adminGuard:CanActivateFn=()=>{
  const a=inject(AuthService);
  return a.isAdmin()?true:inject(Router).createUrlTree(['/login/admin']);
};
