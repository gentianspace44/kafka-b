import { Injectable } from "@angular/core";
import { MatSnackBar } from "@angular/material/snack-bar";

@Injectable({
    providedIn: 'root'
  })
export class Utilities{
    
    static showSnackbar(snackBar: MatSnackBar, message: string, action?: string): void{

        snackBar.open(message, action, {
            duration: 2000, // Duration in milliseconds
          });
    }
}