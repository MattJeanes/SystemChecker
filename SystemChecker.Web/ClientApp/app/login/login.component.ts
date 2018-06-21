import { Component, OnInit } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { first } from "rxjs/operators";
import { AppService, UtilService } from "../services";

@Component({
    templateUrl: "./login.template.html",
    styleUrls: ["./login.style.scss"],
})
export class LoginComponent implements OnInit {
    public returnUrl?: string;
    public username: string;
    public password: string;
    public auto: boolean = false;

    constructor(private appService: AppService, private activatedRoute: ActivatedRoute, private utilService: UtilService, private router: Router) { }

    public async ngOnInit() {
        if (this.appService.isAuthed()) {
            this.utilService.alert("Login", "You are already logged in");
            this.router.navigate(["/"]);
            return;
        }
        const queryParams = await this.activatedRoute.queryParams.pipe(first()).toPromise();
        this.returnUrl = queryParams.return;
        try {
            this.auto = true;
            const result = await this.appService.login();
            if (result.Success) { this.return(); }
            if (result.InitRequired) {
                this.init();
            }
        } catch (e) {
            console.error(e);
            this.utilService.alert("Failed auto-login", e.toString());
        } finally {
            this.auto = false;
        }
    }

    public async login() {
        try {
            const result = await this.appService.login(this.username, this.password);
            if (result.Success) {
                this.return();
            } else if (result.InitRequired) {
                this.init();
            } else {
                throw new Error(result.Error);
            }
        } catch (e) {
            console.error(e);
            this.utilService.alert("Login failed", e.toString());
        }
    }

    private return() {
        if (this.returnUrl) {
            this.router.navigateByUrl(this.returnUrl);
        } else {
            this.router.navigate(["/"]);
        }
    }

    private init() {
        this.router.navigate(["/init"], {
            queryParams: {
                return: this.returnUrl,
            },
        });
    }
}
