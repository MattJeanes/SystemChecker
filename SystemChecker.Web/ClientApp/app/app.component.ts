import { Component } from "@angular/core";

import { MessageService } from "./services";

@Component({
    selector: "systemchecker",
    templateUrl: "./app.template.html",
})
export class AppComponent {
    constructor(public messageService: MessageService) { }
}
