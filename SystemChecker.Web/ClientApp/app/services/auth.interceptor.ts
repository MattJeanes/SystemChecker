import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest, HttpResponse } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs/Observable";
import { AppService } from "./app.service";

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
    constructor(private appService: AppService) { }

    public intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        return next.handle(req).map((resp: any) => {
            if (resp instanceof HttpResponse) {
                const newToken = resp.headers.get("X-Token");
                if (newToken) {
                    this.appService.setToken(newToken);
                }
            }
            return resp;
        });
    }
}
