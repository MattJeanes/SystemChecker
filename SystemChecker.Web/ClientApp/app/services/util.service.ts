// Util

import { Injectable } from "@angular/core";
import { Event, NavigationEnd, Router } from "@angular/router";
import { TdDialogService } from "@covalent/core";
import { filter, first } from "rxjs/operators";

@Injectable()
export class UtilService {
    private currentUrl: string;
    private history: string[] = [];
    private ignoreNavigation: boolean = false;
    constructor(private dialogService: TdDialogService, private router: Router) {
        this.router.events
            .pipe(filter((e: Event) => e instanceof NavigationEnd))
            .subscribe((e: NavigationEnd) => {
                if (!this.ignoreNavigation && this.currentUrl) {
                    this.history.push(this.currentUrl);
                }
                this.currentUrl = e.urlAfterRedirects;
            });
    }
    public async back() {
        if (this.history.length > 0) {
            try {
                this.ignoreNavigation = true;
                const url = this.history[this.history.length - 1];
                const success = await this.router.navigateByUrl(url);
                if (success) {
                    this.history.pop();
                }
            } finally {
                this.ignoreNavigation = false;
            }
        } else {
            this.router.navigate(["/"]);
        }
    }
    public alert(title: string, message: string) {
        return this.dialogService.openAlert({
            title,
            message,
        })
            .afterClosed()
            .pipe(first())
            .toPromise() as Promise<void>;
    }
    public prompt(title: string, message: string) {
        return this.dialogService.openPrompt({
            title,
            message,
        })
            .afterClosed()
            .pipe(first())
            .toPromise() as Promise<string | undefined>;
    }
    public accessDenied() {
        return this.alert("Access denied", "You do not have access to this feature");
    }
    public async confirm(title?: string, message?: string) {
        return this.dialogService.openConfirm({
            title: title ? title : "Confirm action",
            message: message ? message : "Please confirm action",
            acceptButton: "Confirm",
        })
            .afterClosed()
            .pipe(first())
            .toPromise() as Promise<boolean>;
    }
    public async confirmNavigation() {
        return this.confirm("Are you sure?", "You have unsaved changes, are you sure you want to navigate away?");
    }
    public wait(ms: number) {
        return new Promise<void>((resolve, reject) => setTimeout(resolve, ms));
    }
    public equals(x: any, y: any) {
        if (x === y) {
            return true;
        }
        // if both x and y are null or undefined and exactly the same
        if (!(x instanceof Object) || !(y instanceof Object)) {
            return false;
        }
        // if they are not strictly equal, they both need to be Objects
        if (x.constructor !== y.constructor) {
            return false;
        }
        // they must have the exact same prototype chain, the closest we can do is
        // test there constructor.

        let p;
        for (p in x) {
            if (!x.hasOwnProperty(p)) {
                continue;
            }
            // other properties were tested using x.constructor === y.constructor
            if (!y.hasOwnProperty(p)) {
                return false;
            }
            // allows to compare x[ p ] and y[ p ] when set to undefined
            if (x[p] === y[p]) {
                continue;
            }
            // if they have the same strict value or identity then they are equal
            if (typeof (x[p]) !== "object") {
                return false;
            }
            // Numbers, Strings, Functions, Booleans must be strictly equal
            if (!this.equals(x[p], y[p])) {
                return false;
            }
        }
        for (p in y) {
            if (y.hasOwnProperty(p) && !x.hasOwnProperty(p)) {
                return false;
            }
        }
        return true;
    }
    public group<T>(array: T[], key: (item: T) => any): Array<{ key: string, data: T[] }> {
        const map = {};
        array.map(e => ({ k: key(e), d: e })).forEach(e => {
            map[e.k] = map[e.k] || [];
            map[e.k].push(e.d);
        });
        return Object.keys(map).map(k => ({ key: k, data: map[k] }));
    }
}
