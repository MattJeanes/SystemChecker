import { Component, OnInit } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { first } from "rxjs/operators";
import { AppService, UtilService } from "../services";

@Component({
    templateUrl: "./init.template.html",
    styleUrls: ["./init.style.scss"],
})
export class InitComponent implements OnInit {
    public returnUrl?: string;
    public loading: boolean;
    public username: string;
    public password: string;
    public confirmPassword: string;

    constructor(private appService: AppService, private activatedRoute: ActivatedRoute, private utilService: UtilService, private router: Router) { }

    public async ngOnInit() {
        try {
            this.loading = true;
            const queryParams = await this.activatedRoute.queryParams.pipe(first()).toPromise();
            this.returnUrl = queryParams.return;
            const result = await this.appService.getInit();
            if (!result.Required) {
                this.return();
            }
        } catch (e) {
            this.utilService.alert("Init failed to load", e.toString());
        } finally {
            this.loading = false;
        }
    }

    public async create() {
        try {
            const result = await this.appService.setInit({
                Username: this.username,
                Password: this.password,
            });
            if (!result.Required) {
                this.return();
            }
        } catch (e) {
            this.utilService.alert("Failed to create user", e.toString());
        }
    }

    private return() {
        if (this.returnUrl) {
            this.router.navigateByUrl(this.returnUrl);
        } else {
            this.router.navigate(["/"]);
        }
    }
}
