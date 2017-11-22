import { AfterViewInit, Component, ViewChild } from "@angular/core";
import { DataTable, SelectItem } from "primeng/primeng";

import { ICheck } from "../app.interfaces";
import { RunCheckComponent } from "../components";
import { AppService, MessageService } from "../services";

@Component({
    templateUrl: "./dashboard.template.html",
    styleUrls: ["./dashboard.style.scss"],
})
export class DashboardComponent implements AfterViewInit {
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
    constructor(private appService: AppService, private messageService: MessageService) { this.loadChecks(); }
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
    public updateActiveFilter() {
        const col = this.dataTable.columns.find(x => x.header === "Active")!;
        this.dataTable.filter(this.activeOption, col.field, col.filterMatchMode);
    }
    public async run(check: ICheck) {
        await this.appService.run(RunCheckComponent, check);
        await this.loadChecks();
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
    }
}
