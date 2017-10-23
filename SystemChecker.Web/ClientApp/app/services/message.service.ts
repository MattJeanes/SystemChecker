import { Injectable } from "@angular/core";
import { Message } from "primeng/primeng";

@Injectable()
export class MessageService {
    public messages: Message[] = [];
    public addMessage(message: Message) {
        this.messages.push(message);
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
