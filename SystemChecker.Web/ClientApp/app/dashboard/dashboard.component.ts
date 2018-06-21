import { Component, NgZone, OnDestroy, OnInit, ViewChild } from "@angular/core";
import { Router } from "@angular/router";
import { HttpTransportType, HubConnectionBuilder } from "@aspnet/signalr";

import { CheckResultStatus } from "../app.enums";
import { ICheck, ICheckGroup, ICheckType, IEnvironment, ISettings } from "../app.interfaces";
import { RunCheckComponent } from "../components";
import { AppService, MessageService } from "../services";
import { MatTableDataSource, MatSort } from "@angular/material";

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
    @ViewChild("sort") public sort: MatSort;

    public filter: string;
    public activeOnly: boolean = true;
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
    private hubReady: boolean = false;
    private loading: boolean = false;
    constructor(private appService: AppService, private messageService: MessageService, private ngZone: NgZone, private router: Router) {
        this.hub.on("check", () => {
            if (this.loading) { return; }
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
            console.log(this.sort);
            this.dataSource.filterPredicate = (check, value) => {
                return `${check.Name}
${check.Active ? "Yes" : "No"}
${check.GroupID ? this.checkGroupLookup[check.GroupID].Name : "None"}
${this.environmentLookup[check.EnvironmentID!].Name}
${this.typeLookup[check.TypeID!].Name}
${CheckResultStatus[check.LastResultStatus!]}
`.toLowerCase().includes(value);
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
                        return this.environmentLookup[check.EnvironmentID!].Name
                    case "type":
                        return this.typeLookup[check.TypeID!].Name
                    case "lastResultStatus":
                        return CheckResultStatus[check.LastResultStatus!]
                    default:
                        return check[header];
                }
            };
            this.updateCharts();
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
    }
    public ngOnDestroy() {
        if (this.hubReady) {
            this.hub.stop();
        }
    }
    public applyFilter() {
        this.dataSource.filter = this.filter.replace(/ /g, "").toLowerCase();
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
            this.checks.filter(y => y.EnvironmentID === x.ID && (this.activeOnly === false || y.Active)).forEach(x => {
                const status = x.LastResultStatus;
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
            this.filter = `${this.environmentLookup[results.environmentID].Name} ${CheckResultStatus[selected.type!]}`;
            this.applyFilter();
        }
    }
    public onCheckSelected(event: { data: ICheck }) {
        this.router.navigate(["/details", event.data.ID]);
    }
    public setEnvironment(id: number) {
        this.filter = this.environmentLookup[id].Name;
        this.applyFilter();
    }
    public trackChart(index: number, chart: IChart) {
        return chart ? chart.environmentID : undefined;
    }
}
