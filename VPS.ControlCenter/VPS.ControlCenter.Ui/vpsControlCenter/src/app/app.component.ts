import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule, RouterOutlet } from '@angular/router';
import {MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { TokenService } from '../app/services/token.service';
import { LoaderComponent } from './components/loader/loader.component';
import { Utilities } from './helpers/Utilities';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, MatToolbarModule, MatButtonModule, RouterModule, LoaderComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'vpsControlCenter';
constructor(private tokenSvc: TokenService, private router: Router, private snackBar: MatSnackBar){}
  isAuthenticated(): boolean {
    return this.tokenSvc.isLoggedIn(); // Implement this method in your authentication service
  }
  Logout(){
    this.tokenSvc.clearTokens();
    
    Utilities.showSnackbar(this.snackBar, "Logged out successfully.", '');
    this.router.navigate(['/auth/login'])
    
  }
}
