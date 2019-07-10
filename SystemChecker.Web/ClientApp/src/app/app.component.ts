import { Component, OnInit } from "@angular/core";

import { MessageService, PageService } from "./services";

@Component({
    selector: "app-systemchecker",
    templateUrl: "./app.template.html",
    styleUrls: ["./app.style.scss"],
})
export class AppComponent implements OnInit {

    get theme(): string { return localStorage.theme; }
    set theme(value: string) { localStorage.theme = value; }

    private activeTheme?: string;

    constructor(
        public messageService: MessageService,
        public pageService: PageService,
    ) { }

    public ngOnInit() {
        this.updateTheme(this.theme);
    }

    public switchTheme() {
        if (!this.theme || this.theme === "light") {
            this.theme = "dark";
        } else {
            this.theme = "light";
        }
        this.updateTheme(this.theme);
    }

    public updateTheme(theme: string) {
        const htmlTag = document.getElementsByTagName("html")[0];
        htmlTag.classList.add(theme);
        if (this.activeTheme) {
            htmlTag.classList.remove(this.activeTheme);
        }
        this.activeTheme = theme;
    }
}
