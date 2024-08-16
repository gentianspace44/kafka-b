import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class VoucherProviderService {
  private apiUrl = environment.apiURL; // Replace with your actual API base URL

  constructor(private http: HttpClient) { }

  getProviders(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/getProviders`);
  }

  createProvider(provider: any): Observable<number> {
    return this.http.post<number>(`${this.apiUrl}/CreateProvider`, provider);
  }

  updateProvider(provider: any): Observable<boolean> {
    return this.http.put<boolean>(`${this.apiUrl}/UpdateProvider`, provider);
  }
  singleProviderUpdate(provider: any): Observable<boolean> {
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      // Add other headers if needed
    });

    return this.http.put<any>(`${this.apiUrl}/SingleProviderUpdate`, provider, { headers: headers });
  }
  bulkProviderUpdate(providers: any): Observable<boolean> {
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      // Add other headers if needed
    });
    const jsonData = JSON.stringify(providers)
    return this.http.post<any>(`${this.apiUrl}/BulkProviderUpdate`, jsonData, { headers: headers });
  }
}
