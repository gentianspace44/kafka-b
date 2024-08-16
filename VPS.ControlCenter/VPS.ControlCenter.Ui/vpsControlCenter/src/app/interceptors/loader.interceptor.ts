import { Injectable, inject } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor, HttpInterceptorFn } from '@angular/common/http';
import { Observable, finalize } from 'rxjs';
import { LoaderService } from '../services/loader.service';

@Injectable()
 export class loaderInterceptor implements HttpInterceptor {
  constructor(private loaderSvc: LoaderService){}


  intercept(req: HttpRequest<any>, handler: HttpHandler): Observable<HttpEvent<any>> {
   
    this.loaderSvc.show();

    return handler.handle(req).pipe(
      finalize(() => {
        this.loaderSvc.hide();
      })
    );
  }
}