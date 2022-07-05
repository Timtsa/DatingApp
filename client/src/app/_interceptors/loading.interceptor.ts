import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from '@angular/common/http';
import { Observable } from 'rxjs';
import { BusyService } from '../_services/busy.service';
import { delay, finalize } from 'rxjs/operators';

@Injectable()
export class LoadingInterceptor implements HttpInterceptor {

  constructor(private busyService: BusyService) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    this.busyService.busy()
    return next.handle(request).pipe(
  
      finalize(()=>{
        this.busyService.idle();
      })
    );
  }
}
// docker run --name dev -e POSTGRES_USER=appuser  -e POSTGRES_PASSWORD=Pa$$w0rd  -p 5532:5434 -d postgres:latest