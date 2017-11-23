// Util

import { Injectable } from "@angular/core";
import { TdDialogService } from "@covalent/core";

@Injectable()
export class UtilService {
    constructor(private dialogService: TdDialogService) { }
    public alert(title: string, message: string) {
        return this.dialogService.openAlert({
            title,
            message,
        })
            .afterClosed()
            .first()
            .toPromise() as Promise<void>;
    }
    public prompt(title: string, message: string) {
        return this.dialogService.openPrompt({
            title,
            message,
        })
            .afterClosed()
            .first()
            .toPromise() as Promise<string | undefined>;
    }
    public accessDenied() {
        return this.alert("Access denied", "You do not have access to this feature");
    }
    public async confirm(title?: string, message?: string) {
        return this.dialogService.openConfirm({
            title: title ? title : "Confirm action",
            message: message ? message : "Please confirm action",
        })
            .afterClosed()
            .first()
            .toPromise() as Promise<boolean>;
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
}
