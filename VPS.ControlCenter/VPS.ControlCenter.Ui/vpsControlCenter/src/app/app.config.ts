import { ApplicationConfig } from '@angular/core';
import { UrlSerializer, provideRouter, withViewTransitions } from '@angular/router';

import { routes } from './app.routes';
import { provideAnimations } from '@angular/platform-browser/animations';
import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptors, withInterceptorsFromDi } from '@angular/common/http';
import { httpInterceptor,   } from './interceptors/http.interceptor';
import { loaderInterceptor,  } from './interceptors/loader.interceptor';
import { LowerCaseUrlSerializer } from './helpers/LowercaseUrlSerializer';

export const appConfig: ApplicationConfig = {
  providers: [provideRouter(routes, withViewTransitions()), 
    provideAnimations(), 
    provideHttpClient(withInterceptors([ ]), withInterceptorsFromDi()),
    {provide: HTTP_INTERCEPTORS, useClass: httpInterceptor, multi: true},
    {provide: HTTP_INTERCEPTORS, useClass: loaderInterceptor, multi: true},
    {
      provide: UrlSerializer,
      useClass: LowerCaseUrlSerializer
  }
  ]
};
