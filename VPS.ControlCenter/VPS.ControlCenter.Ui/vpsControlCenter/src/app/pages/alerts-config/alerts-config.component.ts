import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatTableDataSource } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatRadioModule } from '@angular/material/radio';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { VoucherProviderService } from '../../services/voucher-provider.service';
import { FormsModule } from '@angular/forms';
import { MatChipsModule } from '@angular/material/chips';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { AlertService } from '../../services/alert.service';
import { Alert } from '../../interfaces/alert';
import { Utilities } from '../../helpers/Utilities';
import { TokenService } from '../../services/token.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-alerts-config',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatChipsModule, MatButtonModule, MatRadioModule, MatCheckboxModule, FormsModule, MatIconModule, MatInputModule, MatFormFieldModule, MatSnackBarModule, MatCardModule],
  templateUrl: './alerts-config.component.html',
  styleUrl: './alerts-config.component.css'
})

export class AlertsConfigComponent implements OnInit {
  displayedColumns: string[] = ['message', 'order'];
  data!: Alert[];
  constructor(private _alertService: AlertService, private snackBar: MatSnackBar, private tokenSvc: TokenService, private router: Router) {
    // this.hubConnection = new HubConnectionBuilder()
    // .withUrl('https://localhost:44377/settingshub') // Adjust the URL to match your SignalR hub endpoint
    // //.withAutomaticReconnect()
    // .build();
  }

  ngOnInit(): void {
    this.fetchData();

  }
  fetchData(): void {
    // Replace the URL with your actual API endpoint

    this._alertService.getAlerts().subscribe((response: any) => {
      this.data = response;
    });
  }
  saveAll() {
    this._alertService.updateAlerts(this.data).subscribe((response: any) => {
      console.log(response);
      this.fetchData();
      Utilities.showSnackbar(this.snackBar,"Saved successfully.", '');
    });
  }

  openSnackBar(message: string, action: string) {
    this.snackBar.open(message, action, {
      duration: 2000, // Duration in milliseconds
    });
  }
  
remove(alert: Alert){
  const index = this.data.indexOf(alert);
  if (index !== -1) {
    this.data.splice(index, 1);
  }
}
addNew(){
  const newItem: Alert = { message: '', order: this.data.length + 1, isVisible : false, title: '' }
  this.data.push(newItem);
}
}
