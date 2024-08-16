import { HttpErrorResponse, HttpEvent, HttpEventType, HttpHandler, HttpInterceptor, HttpInterceptorFn, HttpRequest, HttpResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { TokenService } from '../services/token.service';
import { Observable, catchError, of, tap, throwError } from 'rxjs';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Utilities } from '../helpers/Utilities';


@Injectable()
 export class httpInterceptor implements HttpInterceptor {
  constructor(private tokenSvc: TokenService, private router: Router, private snackbar: MatSnackBar){}
  private handleAuthError(err: HttpErrorResponse): Observable<any> {
    console.log('Request URL: ' );
    //handle your auth error or rethrow
    if (err.status === 401 || err.status === 403) {
      //this.tokenSvc.clearTokens();
      Utilities.showSnackbar(this.snackbar, "Either session expired or you're not authorized to perform this operation. Please login as Super Admin.")
        //navigate /delete cookies or whatever
      //  this.router.navigateByUrl(`/Auth/Login`);
        // if you've caught / handled the error, you don't want to rethrow it unless you also want downstream consumers to have to handle it as well.
        return of(err.message); // or EMPTY may be appropriate here
    }
    return throwError(err);
}

  intercept(req: HttpRequest<any>, handler: HttpHandler): Observable<HttpEvent<any>> {
    let token = this.tokenSvc.getToken();
    if  (token !== undefined && token !== null){
      const cloned = req.clone({
        setHeaders: {
          authorization: 'Bearer ' +token,
        },
      });
     return handler.handle(cloned).pipe(catchError(x=> this.handleAuthError(x)));
    } 
    console.log('Request URL: ' + req.url);
    return handler.handle(req);
  }
}