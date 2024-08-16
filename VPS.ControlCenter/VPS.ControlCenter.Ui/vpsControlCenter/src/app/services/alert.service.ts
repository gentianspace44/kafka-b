import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AlertService {

  private apiUrl = environment.apiURL; // Replace with your actual API base URL

  constructor(private http: HttpClient) { }

  getAlerts(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/api/Alerts/GetList`);
  }
  updateAlerts(provider: any): Observable<number> {
    return this.http.post<number>(`${this.apiUrl}/api/Alerts/UpdateList`, provider);
  }
}
