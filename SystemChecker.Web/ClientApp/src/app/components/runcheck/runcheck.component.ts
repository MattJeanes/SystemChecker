import { Component, Inject, OnInit } from "@angular/core";
import { MAT_DIALOG_DATA, MatDialogRef } from "@angular/material";
import { CheckLogType } from "../../app.enums";
import { ICheck, IRunLog } from "../../app.interfaces";
import { AppService } from "../../services";

@Component({
    templateUrl: "./runcheck.template.html",
    styleUrls: ["./runcheck.style.scss"],
})
export class RunCheckComponent implements OnInit {
    public check: ICheck;
    public log: IRunLog[] = [];
    public running: boolean = false;
    public CheckLogType = CheckLogType;
    constructor(
        private appService: AppService, private dialogRef: MatDialogRef<RunCheckComponent>,
        @Inject(MAT_DIALOG_DATA) public data: any) { }
    public ngOnInit() {
        this.check = this.data.check;
        this.run();
    }
    public async run() {
        try {
            this.running = true;
            if (this.log.length > 0) {
                this.log.push({ Type: CheckLogType.Info, Message: "Re-running check.." });
            } else {
                this.log.push({ Type: CheckLogType.Info, Message: "Running check.." });
            }
            const runLog = await this.appService.startRun(this.check.ID);
            for (const log of runLog) {
                this.log.push(log);
            }
        } catch (e) {
            this.log.push({ Type: CheckLogType.Error, Message: e.toString() });
        } finally {
            this.running = false;
        }
    }
    public close() {
        if (this.running) { return; }
        this.dialogRef.close();
    }
    public clear() {
        this.log = [];
    }
}
