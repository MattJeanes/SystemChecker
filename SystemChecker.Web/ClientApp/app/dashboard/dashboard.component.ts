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
    public run(check: ICheck) {
        this.appService.run(RunCheckComponent, check);
    }
}
