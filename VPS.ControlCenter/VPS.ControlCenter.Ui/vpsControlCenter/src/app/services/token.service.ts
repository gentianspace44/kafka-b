import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class TokenService {
  isLoggedIn(): boolean {
    const jsonString = localStorage.getItem('fullToken');
    if  (jsonString !== null && jsonString !== undefined){
      var obj = JSON.parse(jsonString);

      return true;
    }

return false;
  }

  constructor() { }

  setFullJson(val: string){
    localStorage.setItem("fullToken", val)
  }
  setToken(val: string){
    localStorage.setItem("token", val);
  }
  
  setRefreshToken(val: string){
    localStorage.setItem("refreshToken", val);
  }
  
  getToken(){
    return localStorage.getItem("token");
  }
  getRefreshToken(){
   return localStorage.getItem("refreshToken");
  }

  clearTokens(){
    localStorage.removeItem("token");
    localStorage.removeItem("refreshToken");
    localStorage.removeItem("fullToken");
  }
}
