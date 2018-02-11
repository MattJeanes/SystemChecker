import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { MatDialog } from "@angular/material";
import { ICheck, ICheckDetail, ICheckNotificationType, ICheckResults, ICheckType, IContactType, IRunLog, ISettings, ISlackChannel, ISubCheckType } from "../app.interfaces";

import * as moment from "moment";

@Injectable()
export class AppService {
    constructor(private httpClient: HttpClient, private dialogService: MatDialog) { }
    public async get(id: number, simpleStatus?: boolean) {
        return await this.httpClient.get<ICheck>(`/api/${id}` + (typeof simpleStatus !== "undefined" ? "/" + simpleStatus.toString() : "")).first().toPromise();
    }
    public async getAll(simpleStatus?: boolean) {
        const checks = await this.httpClient.get<ICheck[]>("/api/" + (typeof simpleStatus !== "undefined" ? simpleStatus.toString() : "")).first().toPromise();
        return checks;
    }
    public async getDetails(id: number, includeResults?: boolean) {
        const check = await this.httpClient.get<ICheckDetail>("/api/details/" + id.toString() + (typeof includeResults !== "undefined" ? "/" + includeResults.toString() : "")).first().toPromise();
        if (!includeResults) {
            delete check.Results;
        }
        return check;
    }
    public async getResults(id: number): Promise<ICheckResults>;
    public async getResults(id: number, dateFrom: Date, dateTo: Date): Promise<ICheckResults>;
    public async getResults(id: number, dateFrom?: Date, dateTo?: Date) {
        return await this.httpClient.get<ICheckResults>("/api/results/" + id.toString()
            + (typeof dateFrom !== "undefined" ? "/" + moment(dateFrom).format("YYYY-MM-DD") : "")
            + (typeof dateTo !== "undefined" ? "/" + moment(dateTo).format("YYYY-MM-DD") : "")).first().toPromise();
    }
    public async edit(check: ICheckDetail) {
        check = await this.httpClient.post<ICheckDetail>("/api", check).first().toPromise();
        delete check.Results;
        return check;
    }
    public async delete(id: number) {
        return await this.httpClient.delete<boolean>(id.toString()).first().toPromise();
    }
    public async getTypes() {
        return await this.httpClient.get<ICheckType[]>("/api/types").first().toPromise();
    }
    public async getSettings() {
        return await this.httpClient.get<ISettings>("/api/settings").first().toPromise();
    }
    public async setSettings(settings: ISettings) {
        return await this.httpClient.post<ISettings>("/api/settings", settings).first().toPromise();
    }
    public async startRun(id: number) {
        return await this.httpClient.post<IRunLog[]>(`/api/run/${id}`, undefined).first().toPromise();
    }
    public async getSubCheckTypes(checkTypeID: number) {
        return await this.httpClient.get<ISubCheckType[]>(`/api/subchecktypes/${checkTypeID}`).first().toPromise();
    }
    public async getCheckNotificationTypes() {
        return await this.httpClient.get<ICheckNotificationType[]>("/api/checknotificationtypes").first().toPromise();
    }
    public async getSlackChannels() {
        return await this.httpClient.get<ISlackChannel[]>("/api/slackchannels").first().toPromise();
    }
    public async getContactTypes() {
        return await this.httpClient.get<IContactType[]>("/api/contacttypes").first().toPromise();
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
        return await this.httpClient.post<{ valid: boolean, next?: string; error?: string }>("/api/cron" + (typeof validateOnly !== "undefined" ? "/" + validateOnly.toString() : ""), cron).first().toPromise();
    }
}
