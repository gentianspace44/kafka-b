import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../services/auth.service';
import { TokenService } from '../../../services/token.service';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatRadioModule } from '@angular/material/radio';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { Router } from '@angular/router';
import { Utilities } from '../../../helpers/Utilities';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatChipsModule, MatButtonModule, MatRadioModule, MatCheckboxModule, FormsModule, MatIconModule, MatInputModule, MatFormFieldModule, MatSnackBarModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
})
export class LoginComponent {

  title = 'Login';
  constructor(private authSvc: AuthService, private tokenSvc: TokenService, private router: Router, private snackBar: MatSnackBar){}

   username: string = '';
   password: string = '';

  login() {
    this.authSvc.login(this.username, this.password).subscribe(d => {
      this.tokenSvc.setToken(d.accessToken.token);
      this.tokenSvc.setRefreshToken(d.refreshToken.token);
      this.tokenSvc.setFullJson(JSON.stringify(d));
      
    Utilities.showSnackbar(this.snackBar, "Logged in successfully.", '');
      this.router.navigate(['/home'])
    });
  }
}
