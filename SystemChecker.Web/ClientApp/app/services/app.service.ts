import { Injectable } from "@angular/core";
import { HttpClient, HttpHeaders, HttpParams, HttpErrorResponse } from "@angular/common/http";
import { MatDialog } from "@angular/material";
import { ICheck, ICheckDetail, ICheckNotificationType, ICheckResults, ICheckType, IContactType, IRunLog, ISettings, ISlackChannel, ISubCheckType } from "../app.interfaces";

import * as moment from "moment";

export class BaseWebService {
    constructor(private httpClient: HttpClient) { }
    public async get<T>(url: string, options?: {
        headers?: HttpHeaders | {
            [header: string]: string | string[];
        };
        observe?: 'body';
        params?: HttpParams | {
            [param: string]: string | string[];
        };
        reportProgress?: boolean;
        responseType?: 'json';
        withCredentials?: boolean;
    }): Promise<T> {
        try {
            const response = await this.httpClient.get<T>(url, options).first().toPromise();
            return response;
        } catch (e) {
            throw this.handleError(e);
        }
    }

    public async post<T>(url: string, body: any | null, options?: {
        headers?: HttpHeaders | {
            [header: string]: string | string[];
        };
        observe?: 'body';
        params?: HttpParams | {
            [param: string]: string | string[];
        };
        reportProgress?: boolean;
        responseType?: 'json';
        withCredentials?: boolean;
    }): Promise<T> {
        try {
            const response = await this.httpClient.post<T>(url, body, options).first().toPromise();
            return response;
        } catch (e) {
            throw this.handleError(e);
        }
    }

    public async delete<T>(url: string, options?: {
        headers?: HttpHeaders | {
            [header: string]: string | string[];
        };
        observe?: 'body';
        params?: HttpParams | {
            [param: string]: string | string[];
        };
        reportProgress?: boolean;
        responseType?: 'json';
        withCredentials?: boolean;
    }): Promise<T> {
        try {
            const response = await this.httpClient.delete<T>(url, options).first().toPromise();
            return response;
        } catch (e) {
            throw this.handleError(e);
        }
    }

    private handleError(e: any): Error {
        let errMsg: string;
        let logged: boolean = false;
        if (e instanceof HttpErrorResponse) {
            const err = (e.error && (e.error.error || JSON.stringify(e.error))) || e.message;
            if (e.error && e.error.stack) {
                console.error(`${err}\n${e.error.stack}`);
                logged = true;
            }
            errMsg = err;
        } else {
            errMsg = e.toString();
        }
        if (!logged) {
            console.error(errMsg);
        }
        return new Error(errMsg);
    }
}

@Injectable()
export class AppService {
    private readonly baseWebService = new BaseWebService(this.httpClient);
    constructor(private httpClient: HttpClient, private dialogService: MatDialog) { }
    public async get(id: number, simpleStatus?: boolean) {
        return await this.baseWebService.get<ICheck>(`/api/${id}` + (typeof simpleStatus !== "undefined" ? "/" + simpleStatus.toString() : ""));
    }
    public async getAll(simpleStatus?: boolean) {
        const checks = await this.baseWebService.get<ICheck[]>("/api/" + (typeof simpleStatus !== "undefined" ? simpleStatus.toString() : ""));
        return checks;
    }
    public async getDetails(id: number, includeResults?: boolean) {
        const check = await this.baseWebService.get<ICheckDetail>("/api/details/" + id.toString() + (typeof includeResults !== "undefined" ? "/" + includeResults.toString() : ""));
        if (!includeResults) {
            delete check.Results;
        }
        return check;
    }
    public async getResults(id: number): Promise<ICheckResults>;
    public async getResults(id: number, dateFrom: Date, dateTo: Date): Promise<ICheckResults>;
    public async getResults(id: number, dateFrom?: Date, dateTo?: Date) {
        return await this.baseWebService.get<ICheckResults>("/api/results/" + id.toString()
            + (typeof dateFrom !== "undefined" ? "/" + moment(dateFrom).format("YYYY-MM-DD") : "")
            + (typeof dateTo !== "undefined" ? "/" + moment(dateTo).format("YYYY-MM-DD") : ""));
    }
    public async edit(check: ICheckDetail) {
        check = await this.baseWebService.post<ICheckDetail>("/api", check);
        delete check.Results;
        return check;
    }
    public async delete(id: number) {
        return await this.baseWebService.delete<boolean>(id.toString());
    }
    public async getTypes() {
        return await this.baseWebService.get<ICheckType[]>("/api/types");
    }
    public async getSettings() {
        return await this.baseWebService.get<ISettings>("/api/settings");
    }
    public async setSettings(settings: ISettings) {
        return await this.baseWebService.post<ISettings>("/api/settings", settings);
    }
    public async startRun(id: number) {
        return await this.baseWebService.post<IRunLog[]>(`/api/run/${id}`, undefined);
    }
    public async getSubCheckTypes(checkTypeID: number) {
        return await this.baseWebService.get<ISubCheckType[]>(`/api/subchecktypes/${checkTypeID}`);
    }
    public async getCheckNotificationTypes() {
        return await this.baseWebService.get<ICheckNotificationType[]>("/api/checknotificationtypes");
    }
    public async getSlackChannels() {
        return await this.baseWebService.get<ISlackChannel[]>("/api/slackchannels");
    }
    public async getContactTypes() {
        return await this.baseWebService.get<IContactType[]>("/api/contacttypes");
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
        return await this.baseWebService.post<{ valid: boolean, next?: string; error?: string }>("/api/cron" + (typeof validateOnly !== "undefined" ? "/" + validateOnly.toString() : ""), cron);
    }
}
