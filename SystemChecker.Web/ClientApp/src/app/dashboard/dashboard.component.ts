import { Component, NgZone, OnDestroy, OnInit, ViewChild } from "@angular/core";
import { MatPaginator, MatSort, MatTableDataSource } from "@angular/material";
import { HttpTransportType, HubConnectionBuilder } from "@aspnet/signalr";
import { PageVisibilityService } from "ngx-page-visibility";
import { Subscription } from "rxjs";
import * as store from "store";

import { CheckResultStatus } from "../app.enums";
import { ICheck, ICheckGroup, ICheckType, IEnvironment, ISettings } from "../app.interfaces";
import { RunCheckComponent } from "../components";
import { AppService, MessageService } from "../services";

interface IChart {
    name: string;
    environmentID: number;
    results: Array<{
        name: string,
        value: number,
        type: CheckResultStatus | null,
    }>;
}

@Component({
    templateUrl: "./dashboard.template.html",
    styleUrls: ["./dashboard.style.scss"],
})
export class DashboardComponent implements OnInit, OnDestroy {
    public chartColors = {
        domain: ["#5AA454", "#FBC02D", "#A10A28", "#E0E0E0"],
    };
    public chart: IChart[] = [
        {
            name: "Loading",
            environmentID: 0,
            results: [{
                name: "Loading",
                value: 0,
                type: null,
            }],
        }];
    public checks: ICheck[] = [];
    public dataSource: MatTableDataSource<ICheck>;
    public displayedColumns = ["name", "active", "group", "environment", "type", "lastResultStatus", "options"];
    @ViewChild(MatSort) public sort: MatSort;
    @ViewChild(MatPaginator) public paginator: MatPaginator;

    public filter: string;
    public get activeOnly() {
        return store.get("activeOnly", true) as boolean;
    }
    public set activeOnly(value: boolean) {
        store.set("activeOnly", value);
    }
    public environmentLookup: {
        [key: string]: IEnvironment;
    };
    public checkGroupLookup: {
        [key: string]: ICheckGroup;
    };
    public typeLookup: {
        [key: string]: ICheckType;
    };
    public CheckResultStatus = CheckResultStatus;
    public settings: ISettings = {
        Logins: [],
        ConnStrings: [],
        Environments: [{ ID: 0, Name: "Loading" }],
        Contacts: [],
        CheckGroups: [],
        Global: {},
    };
    public types: ICheckType[] = [];
    private hub = new HubConnectionBuilder()
        .withUrl("hub/dashboard", { transport: HttpTransportType.WebSockets })
        .build();
    private hubReady = false;
    private loading = false;
    private newResults = false;
    private subscriptions: Subscription[];
    constructor(
        private appService: AppService,
        private messageService: MessageService,
        private ngZone: NgZone,
        private pageVisibilityService: PageVisibilityService,
    ) {
        this.hub.on("check", () => {
            if (this.loading) { return; }
            if (!this.pageVisibilityService.isPageVisible()) { this.newResults = true; return; }
            // Because this is a call from the server, Angular change detection won't detect it so we must force ngZone to run
            this.ngZone.run(async () => {
                await this.loadChecks();
            });
        });
    }
    public async loadChecks() {
        try {
            if (this.loading) { return; }
            this.loading = true;
            delete this.settings;
            this.settings = await this.appService.getSettings();
            delete this.types;
            this.types = await this.appService.getTypes();

            delete this.environmentLookup;
            this.environmentLookup = {};
            this.settings.Environments.map(x => {
                this.environmentLookup[x.ID] = x;
            });

            delete this.typeLookup;
            this.typeLookup = {};
            this.types.map(x => {
                this.typeLookup[x.ID] = x;
            });

            delete this.checkGroupLookup;
            this.checkGroupLookup = {};
            this.settings.CheckGroups.map(x => {
                this.checkGroupLookup[x.ID] = x;
            });

            delete this.checks;
            this.checks = await this.appService.getAll(true);
            this.dataSource = new MatTableDataSource(this.checks);
            this.dataSource.sort = this.sort;
            this.dataSource.paginator = this.paginator;
            this.dataSource.filterPredicate = (check, valueRaw) => {
                if (this.activeOnly && !check.Active) { return false; }
                if (!valueRaw) { return true; }
                const fields = [
                    `name: ${check.Name}`,
                    `active: ${check.Active ? "Yes" : "No"}`,
                    `group: ${check.GroupID ? this.checkGroupLookup[check.GroupID].Name : "None"}`,
                    `env: ${this.environmentLookup[check.EnvironmentID!].Name}`,
                    `type: ${this.typeLookup[check.TypeID!].Name}`,
                    `status: ${CheckResultStatus[check.LastResultStatus!]}`,
                ].map(x => x.replace(/ /g, "").toLowerCase());
                const values = valueRaw.split(",").filter(x => x);
                for (const value of values) {
                    let match = false;
                    for (const field of fields) {
                        if (field.includes(value)) {
                            match = true;
                        }
                    }
                    if (!match) { return false; }
                }
                return true;
            };
            this.dataSource.sortingDataAccessor = (check, header) => {
                console.log(header);
                switch (header) {
                    case "name":
                        return check.Name;
                    case "active":
                        return check.Active ? "Yes" : "No";
                    case "group":
                        return check.GroupID ? this.checkGroupLookup[check.GroupID].Name : "None";
                    case "environment":
                        return this.environmentLookup[check.EnvironmentID!].Name;
                    case "type":
                        return this.typeLookup[check.TypeID!].Name;
                    case "lastResultStatus":
                        return CheckResultStatus[check.LastResultStatus!];
                    default:
                        return check[header];
                }
            };
            this.updateCharts();
            this.applyFilter();
        } catch (e) {
            this.messageService.error("Failed to load checks", e.toString());
        } finally {
            this.loading = false;
        }
    }
    public async ngOnInit() {
        await this.loadChecks();
        await this.hub.start();
        this.hubReady = true;
        this.subscriptions = [
            this.pageVisibilityService.$onPageVisible.subscribe(() => this.ngZone.run(this.onPageVisible.bind(this))),
        ];
    }
    public ngOnDestroy() {
        if (this.hubReady) {
            this.hub.stop();
        }
        this.subscriptions.forEach(x => x.unsubscribe());
        this.subscriptions = [];
    }
    public applyFilter() {
        this.dataSource.filter = `${(this.activeOnly ? "active:yes," : "")}${(this.filter ? this.filter.replace(/ /g, "").toLowerCase() : "")}`;
    }
    public async run(check: ICheck) {
        await this.appService.run(RunCheckComponent, check);
    }
    public updateCharts() {
        if (!this.settings) { return; }
        this.chart = this.settings.Environments.map<IChart>(x => {
            let success = 0;
            let warning = 0;
            let failed = 0;
            let notRun = 0;
            this.checks.filter(y => y.EnvironmentID === x.ID && (this.activeOnly === false || y.Active)).forEach(z => {
                const status = z.LastResultStatus;
                switch (status) {
                    case CheckResultStatus.Success:
                        success++;
                        break;
                    case CheckResultStatus.Warning:
                        warning++;
                        break;
                    case CheckResultStatus.Failed:
                        failed++;
                        break;
                    case CheckResultStatus.NotRun:
                        notRun++;
                        break;
                }
            });
            return {
                name: x.Name,
                environmentID: x.ID,
                results: [
                    {
                        name: "Successful",
                        value: success,
                        type: CheckResultStatus.Success,
                    },
                    {
                        name: "Warning",
                        value: warning,
                        type: CheckResultStatus.Warning,
                    },
                    {
                        name: "Failed",
                        value: failed,
                        type: CheckResultStatus.Failed,
                    },
                    {
                        name: "Not run",
                        value: notRun,
                        type: CheckResultStatus.NotRun,
                    },
                ],
            };
        });
    }
    public onChartSelect(results: IChart, event: { name: string, value: number }) {
        const selected = results.results.find(x => x.name === event.name);
        if (selected) {
            this.filter = `env:${this.environmentLookup[results.environmentID].Name}, status:${CheckResultStatus[selected.type!]}`;
            this.applyFilter();
        }
    }
    public onActiveOnlyChange() {
        this.updateCharts();
        this.applyFilter();
    }
    public setEnvironment(id: number) {
        this.filter = `env:${this.environmentLookup[id].Name}`;
        this.applyFilter();
    }
    public trackChart(index: number, chart: IChart) {
        return chart ? chart.environmentID : undefined;
    }
    private async onPageVisible() {
        if (this.newResults) {
            this.newResults = false;
            await this.loadChecks();
        }
    }
}
