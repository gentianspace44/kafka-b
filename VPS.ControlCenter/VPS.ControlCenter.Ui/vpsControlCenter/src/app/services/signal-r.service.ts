import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@aspnet/signalr';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection: HubConnection;

  constructor() {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${environment.apiURL}/settingshub`) // Adjust the URL to match your SignalR hub endpoint
      .build();
  }

  startConnection(): Observable<void> {
    return new Observable((observer) => {
      this.hubConnection
        .start()
        .then(() => {
          console.log('Connection started');
          observer.next();
          observer.complete();
        })
        .catch((err) => {
          console.error('Error while starting connection: ' + err);
          observer.error(err);
        });
    });
  }

  onReceiveMessage(): Observable<string> {
    return new Observable((observer) => {
      this.hubConnection.on('ClearCache', (message: string) => {
        console.log('Received message:', message);
        observer.next(message);
      });
    });
  }
}
