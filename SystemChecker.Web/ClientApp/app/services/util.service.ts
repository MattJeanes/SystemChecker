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
}
