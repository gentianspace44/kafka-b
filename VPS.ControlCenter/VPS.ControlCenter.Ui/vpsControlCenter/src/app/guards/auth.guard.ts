import { CanActivateFn, Router } from '@angular/router';
import { TokenService } from '../services/token.service';
import { inject } from '@angular/core';

export const authGuard: CanActivateFn = (route, state) => {
  let svc = inject(TokenService);
if  (svc.getToken() !== undefined && svc.getToken() !== null){
  return true;
}else{
  inject(Router).navigate(['Auth/Login']);
  return false;
}
};
