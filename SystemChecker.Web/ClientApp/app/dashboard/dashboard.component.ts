import { AfterViewInit, Component, NgZone, OnDestroy, OnInit, ViewChild } from "@angular/core";
import { Router } from "@angular/router";

import { DataTable, SelectItem } from "primeng/primeng";

import { CheckResultStatus } from "../app.enums";
import { ICheck, ICheckType, IEnvironment, ISettings } from "../app.interfaces";
import { RunCheckComponent } from "../components";
import { AppService, MessageService } from "../services";

import { HubConnection } from "@aspnet/signalr-client";

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
export class DashboardComponent implements AfterViewInit, OnInit, OnDestroy {
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
    public activeOptions: SelectItem[] = [
        { label: "Yes", value: true },
        { label: "No", value: false },
        { label: "All", value: null },
    ];
    public resultOptions: SelectItem[] = [
        { label: "All", value: null },
        { label: "Successful", value: CheckResultStatus.Success },
        { label: "Warning", value: CheckResultStatus.Warning },
        { label: "Failed", value: CheckResultStatus.Failed },
        { label: "Not run", value: CheckResultStatus.NotRun },
    ];
    public environmentOptions: SelectItem[] = [];
    public typeOptions: SelectItem[] = [];
    public activeOption: boolean | null = true;
    public resultOption: CheckResultStatus | null = null;
    public environmentOption: number | null = null;
    public typeOption: number | null = null;
    public CheckResultStatus = CheckResultStatus;
    public settings: ISettings = {
        Logins: [],
        ConnStrings: [],
        Environments: [{ ID: 0, Name: "Loading" }],
    };
    public types: ICheckType[] = [];
    public environmentLookup: {
        [key: string]: IEnvironment;
    };
    public typeLookup: {
        [key: string]: ICheckType;
    };
    @ViewChild("dt") private dataTable: DataTable;
    private hub = new HubConnection("hub/dashboard");
    private hubReady: boolean = false;
    constructor(private appService: AppService, private messageService: MessageService, private ngZone: NgZone, private router: Router) {
        this.hub.on("check", () => {
            // Because this is a call from the server, Angular change detection won't detect it so we must force ngZone to run
            this.ngZone.run(async () => {
                await this.loadChecks();
            });
        });
    }
    public async loadChecks() {
        try {
            delete this.settings;
            this.settings = await this.appService.getSettings();
            delete this.types;
            this.types = await this.appService.getTypes();

            delete this.environmentLookup;
            this.environmentLookup = {};
            delete this.environmentOptions;
            this.environmentOptions = [{ label: "All", value: null }];
            this.settings.Environments.map(x => {
                this.environmentLookup[x.ID] = x;
                this.environmentOptions.push({ label: x.Name, value: x.ID });
            });

            delete this.typeLookup;
            this.typeLookup = {};
            delete this.typeOptions;
            this.typeOptions = [{ label: "All", value: null }];
            this.types.map(x => {
                this.typeLookup[x.ID] = x;
                this.typeOptions.push({ label: x.Name, value: x.ID });
            });

            delete this.checks;
            this.checks = await this.appService.getAll(true);
            this.updateCharts();
        } catch (e) {
            this.messageService.error("Failed to load checks", e.toString());
        }
    }
    public ngAfterViewInit() {
        setImmediate(() => this.updateActiveFilter());
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
    public updateActiveFilter() {
        const col = this.dataTable.columns.find(x => x.header === "Active")!;
        this.dataTable.filter(this.activeOption, col.field, col.filterMatchMode);
        this.updateCharts();
    }
    public async run(check: ICheck) {
        await this.appService.run(RunCheckComponent, check);
    }
    public updateCharts() {
        this.chart = this.settings.Environments.map<IChart>(x => {
            let success = 0;
            let warning = 0;
            let failed = 0;
            let notRun = 0;
            this.checks.filter(y => y.EnvironmentID === x.ID && (this.activeOption === null || this.activeOption === y.Active)).forEach(x => {
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
    public updateResultFilter() {
        const col = this.dataTable.columns.find(x => x.header === "Last Result Status")!;
        this.dataTable.filter(this.resultOption, col.field, col.filterMatchMode);
    }
    public updateEnvironmentFilter() {
        const col = this.dataTable.columns.find(x => x.header === "Environment")!;
        this.dataTable.filter(this.environmentOption, col.field, col.filterMatchMode);
    }
    public updateTypeFilter() {
        const col = this.dataTable.columns.find(x => x.header === "Type")!;
        this.dataTable.filter(this.typeOption, col.field, col.filterMatchMode);
    }
    public onChartSelect(results: IChart, event: { name: string, value: number }) {
        const selected = results.results.find(x => x.name === event.name);
        if (selected) {
            this.resultOption = selected.type;
            this.environmentOption = results.environmentID;
            this.updateResultFilter();
            this.updateEnvironmentFilter();
        }
    }
    public onCheckSelected(event: { data: ICheck }) {
        this.router.navigate(["/details", event.data.ID]);
    }
    public setEnvironment(id: number) {
        this.environmentOption = id;
        this.resultOption = null;
        this.updateEnvironmentFilter();
        this.updateResultFilter();
    }
    public trackChart(index: number, chart: IChart) {
        return chart ? chart.environmentID : undefined;
    }
}
