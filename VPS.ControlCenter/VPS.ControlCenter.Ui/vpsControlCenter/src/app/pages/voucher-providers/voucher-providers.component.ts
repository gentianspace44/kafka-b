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


import { MatChipsModule } from '@angular/material/chips';
import { FormsModule } from '@angular/forms';

import { MatFormFieldModule } from '@angular/material/form-field';
import { } from '@angular/material/checkbox';
import { Utilities } from '../../helpers/Utilities';
import { VoucherProviderService } from '../../services/voucher-provider.service';
@Component({
  selector: 'app-voucher-providers',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatChipsModule, MatButtonModule, MatRadioModule, MatCheckboxModule, FormsModule, MatIconModule, MatInputModule, MatFormFieldModule, MatSnackBarModule],
  templateUrl: './voucher-providers.component.html',
  styleUrl: './voucher-providers.component.css'
})
export class VoucherProvidersComponent implements OnInit {
  displayedColumns: string[] = ['name', 'Enable', 'Visible', 'UseSxyCreditEndPoint', 'Length', 'MicroServiceUrl', 'SyxCreditServiceUrl', 'actions'];
  data!: MatTableDataSource<any>;
   //private hubConnection: HubConnection;

  constructor(private _voucherProviderService: VoucherProviderService, private snackbar: MatSnackBar) {
    // this.hubConnection = new HubConnectionBuilder()
    // .withUrl('https://localhost:44377/settingshub') // Adjust the URL to match your SignalR hub endpoint
    // //.withAutomaticReconnect()
    // .build();
  }

  ngOnInit(): void {
    this.fetchData();
    // this.signalRService.startConnection().subscribe(() => {
    //   this.signalRService.onReceiveMessage().subscribe((message) => {
    //     console.log('Message received in component:', message);
    //     // Do something with the received message
    //   });
    // });
  }
  fetchData(): void {
    // Replace the URL with your actual API endpoint

    this._voucherProviderService.getProviders().subscribe((response: any) => {
      this.data = new MatTableDataSource(response);
    });
  }
  saveChanges(element: any) {
    this._voucherProviderService.singleProviderUpdate(element).subscribe((response: any) => {
      console.log(response);
      this.fetchData();
      this.openSnackBar('Providers saved successfully', 'Close');
    });
  }

  saveAll() {
    var ob = {
      VoucherProviders: this.data.data
    }
    this._voucherProviderService.bulkProviderUpdate(ob).subscribe((response: any) => {
      console.log(response);
      this.fetchData();
      this.openSnackBar('Providers saved successfully', 'Close');
    });
  }

  openSnackBar(message: string, action: string) {
    Utilities.showSnackbar(this.snackbar, message, action);
  }
}
