import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthModel } from '../interfaces/auth-model';


@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = environment.apiURL; 

  private httpOptions = {
    headers: new HttpHeaders({ 'Content-Type': 'application/json' })
  };
  constructor(private httpClient: HttpClient) { }

  login(username: string, password: string): Observable<AuthModel> {
    return this.httpClient.post<AuthModel>(
      this.apiUrl + '/api/auth/login',
      {
        username,
        password,
      },
      this.httpOptions
    );
  }
}
