import { Injectable } from "@angular/core";
import { MatSnackBar } from "@angular/material";
import { TdDialogService } from "@covalent/core";

export interface IMessage {
    summary: string;
    detail?: string;
    severity: "success" | "error" | "info";
}

@Injectable()
export class MessageService {
    constructor(private snackbar: MatSnackBar, private dialogService: TdDialogService) { }
    public addMessage(message: IMessage) {
        this.snackbar.open(message.summary, (message.detail ? "Details" : "Okay").toUpperCase(), {
            panelClass: `snackbar-${message.severity}`,
            duration: message.severity === "error" ? 5000 : 2000,
        }).onAction().subscribe(() => {
            if (message.detail) {
                this.dialogService.openAlert({
                    title: message.summary,
                    message: message.detail,
                });
            }
        });
    }
    public success(summary: string, detail?: string) {
        this.addMessage({ summary, detail, severity: "success" });
    }
    public error(summary: string, detail?: string) {
        this.addMessage({ summary, detail, severity: "error" });
    }
    public info(summary: string, detail?: string) {
        this.addMessage({ summary, detail, severity: "info" });
    }
}
