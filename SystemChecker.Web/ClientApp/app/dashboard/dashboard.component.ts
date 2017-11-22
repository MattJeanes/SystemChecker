import { AfterViewInit, ChangeDetectorRef, Component, OnDestroy, OnInit, ViewChild } from "@angular/core";
import { DataTable, SelectItem } from "primeng/primeng";

import { ICheck } from "../app.interfaces";
import { RunCheckComponent } from "../components";
import { AppService, MessageService } from "../services";

import { HubConnection } from "@aspnet/signalr-client";

@Component({
    templateUrl: "./dashboard.template.html",
    styleUrls: ["./dashboard.style.scss"],
})
export class DashboardComponent implements AfterViewInit, OnInit, OnDestroy {
    public successChart = {
        colorScheme: {
            domain: ["#5AA454", "#FBC02D", "#A10A28", "#E0E0E0"],
        },
        results: [
            {
                name: "Loading",
                value: 0,
            },
        ],
    };
    public checks: ICheck[] = [];
    public activeOptions: SelectItem[] = [
        { label: "Yes", value: true },
        { label: "No", value: false },
        { label: "All", value: null },
    ];
    public activeOption: boolean | null = true;
    @ViewChild("dt") private dataTable: DataTable;
    private hub = new HubConnection("hub/dashboard");
    private hubReady: boolean = false;
    constructor(private appService: AppService, private messageService: MessageService, private changeDetectorRef: ChangeDetectorRef) {
        this.loadChecks();
        this.hub.on("check", this.loadChecks.bind(this));
    }
    public async loadChecks() {
        try {
            this.checks = await this.appService.getAll();
            this.updateCharts();
        } catch (e) {
            this.messageService.error("Failed to load checks", e.toString());
        }
    }
    public ngAfterViewInit() {
        setImmediate(() => this.updateActiveFilter());
    }
    public async ngOnInit() {
        this.hub.start();
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
    }
    public async run(check: ICheck) {
        await this.appService.run(RunCheckComponent, check);
    }
    public updateCharts() {
        let success = 0;
        let warning = 0;
        let failed = 0;
        let notRun = 0;
        this.checks.forEach(x => {
            const status = x.LastResultStatus;
            if (typeof status !== "undefined") {
                if (status > 1) {
                    warning++;
                } else if (status === 1) {
                    success++;
                } else {
                    failed++;
                }
            } else {
                notRun++;
            }
        });
        this.successChart.results = [
            {
                name: "Successful",
                value: success,
            },
            {
                name: "Warning",
                value: warning,
            },
            {
                name: "Failed",
                value: failed,
            },
            {
                name: "Not run",
                value: notRun,
            },
        ];
        this.changeDetectorRef.detectChanges();
    }
}
