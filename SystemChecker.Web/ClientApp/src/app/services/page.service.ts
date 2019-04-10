// Page Service

import { Injectable } from "@angular/core";
import { Event, NavigationEnd, Router } from "@angular/router";
import { filter } from "rxjs/operators";
import { AppService } from "./app.service";

@Injectable()
export class PageService {
    public title = "Loading..";
    public dashboardLink = false;
    public settingsLink = false;
    private loading: boolean;
    constructor(private router: Router, private appService: AppService) {
        this.router.events
            .pipe(filter((e: Event) => e instanceof NavigationEnd))
            .subscribe((e: NavigationEnd) => {
                let currentRoute = this.router.routerState.root;
                while (currentRoute.children[0] !== undefined) {
                    currentRoute = currentRoute.children[0];
                }
                this.title = currentRoute.snapshot.data.title;
                this.dashboardLink = !currentRoute.snapshot.data.noDashboardLink;
                this.settingsLink = !currentRoute.snapshot.data.noSettingsLink;
            });
    }
    public get username() {
        return this.appService.getUsername();
    }
    public setLoading(loading: boolean) {
        this.loading = loading;
    }
}
