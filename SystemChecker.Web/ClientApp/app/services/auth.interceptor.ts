import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest, HttpResponse } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs/Observable";
import { ErrorObservable } from "rxjs/observable/ErrorObservable";
import { AppService } from "./app.service";
import { UtilService } from "./util.service";

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
    constructor(private appService: AppService, private utilService: UtilService) { }

    public intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        return next.handle(req).map((resp: any) => {
            if (resp instanceof HttpResponse) {
                const newToken = resp.headers.get("X-Token");
                if (newToken) {
                    this.appService.setToken(newToken);
                }
            }
            return resp;
        }).catch((resp: any) => {
            if (resp instanceof HttpErrorResponse) {
                const tokenInvalid = resp.headers.get("X-Token-Invalid");
                if (tokenInvalid) {
                    this.utilService.alert("Invalid token", "Server rejected token, logged out");
                    this.appService.logout();
                }
            }
            return ErrorObservable.create(resp);
        });
    }
}
