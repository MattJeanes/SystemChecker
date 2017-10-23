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
    //private handleResponseBind = this.handleResponse.bind(this);
    constructor(
        private appService: AppService, private dialogRef: MatDialogRef<RunCheckComponent>,
        @Inject(MAT_DIALOG_DATA) public data: any) { }
    public ngOnInit() {
        this.check = this.data.check;
        //this.appService.on("run", this.handleResponseBind);
        this.run();
    }
    public ngOnDestroy() {
        //this.appService.off("run", this.handleResponseBind);
    }
    public run() {
        this.running = true;
        if (this.log.length > 0) {
            this.log.push({ Type: CheckLogType.Info, Message: "\nRe-running check..\n" });
        } else {
            this.log.push({ Type: CheckLogType.Info, Message: "Running check..\n" });
        }
        this.appService.startRun(this.check.ID);
    }
    public close() {
        if (this.running) { return; }
        this.dialogRef.close();
    }
    public clear() {
        this.log = [];
    }
    // private handleResponse(data: IRunLog) {
    //     this.log.push(data);
    //     if (data.Type === CheckLogType.Done) {
    //         this.running = false;
    //     }
    // }
}
