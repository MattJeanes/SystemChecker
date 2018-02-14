import { Component } from "@angular/core";

import { MessageService, PageService } from "./services";

@Component({
    selector: "systemchecker",
    templateUrl: "./app.template.html",
    styleUrls: ["./app.style.scss"],
})
export class AppComponent {
    constructor(public messageService: MessageService, public pageService: PageService) { }
}
