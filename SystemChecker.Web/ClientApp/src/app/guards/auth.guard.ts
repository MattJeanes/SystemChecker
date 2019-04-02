import { Injectable } from "@angular/core";
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot } from "@angular/router";
import { AppService } from "../services";

@Injectable()
export class AuthGuard implements CanActivate {

    constructor(private appService: AppService, private router: Router) { }

    public async canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot) {
        if (this.appService.isAuthed()) {
            return true;
        } else {
            this.router.navigate(["/login"], {
                queryParams: {
                    return: state.url,
                },
            });
            return false;
        }
    }
}
