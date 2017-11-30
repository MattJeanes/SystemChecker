import { Injectable } from "@angular/core";
import { Http } from "@angular/http";
import { MatDialog } from "@angular/material";
import { ICheck, ICheckDetail, ICheckType, IRunLog, ISettings, ISubCheckType } from "../app.interfaces";

import { BaseWebService } from "***REMOVED***";

@Injectable()
export class AppService {
    private webService = new BaseWebService(this.http, "/api");
    constructor(private http: Http, private dialogService: MatDialog) { }
    public async getAll() {
        const checks = await this.webService.get<ICheck[]>();
        return checks;
    }
    public async getDetails(id: number, includeResults?: boolean) {
        const check = await this.webService.get<ICheckDetail>(id.toString() + (typeof includeResults !== "undefined" ? "/" + includeResults.toString() : ""));
        return check;
    }
    public async edit(check: ICheckDetail) {
        return await this.webService.post<ICheckDetail>({ data: check });
    }
    public async delete(id: number) {
        return await this.webService.delete<boolean>(id.toString());
    }
    public async getTypes() {
        return await this.webService.get<ICheckType[]>("types");
    }
    public async getSettings() {
        return await this.webService.get<ISettings>("settings");
    }
    public async setSettings(settings: ISettings) {
        return await this.webService.post<ISettings>({ path: "settings", data: settings });
    }
    public async startRun(id: number) {
        return await this.webService.post<IRunLog[]>(`run/${id}`);
    }
    public async getSubCheckTypes(checkTypeID: number) {
        return await this.webService.get<ISubCheckType[]>(`subchecktypes/${checkTypeID}`);
    }
    public run(component: any, check: ICheck) {
        return this.dialogService.open(component, {
            width: "90%",
            height: "90%",
            panelClass: "panel-dark",
            data: { check },
        })
            .afterClosed()
            .first()
            .toPromise() as Promise<void>;
    }
    public async validateCronExpression(cron: string, validateOnly?: boolean) {
        return await this.webService.post<{ valid: boolean, next?: string; error?: string }>({
            path: "cron" + (typeof validateOnly !== "undefined" ? "/" + validateOnly.toString() : ""),
            data: cron,
        });
    }
}
