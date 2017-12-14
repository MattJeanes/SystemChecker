import { Component, OnInit } from "@angular/core";
import { FormArray, FormBuilder, FormGroup, Validators } from "@angular/forms";
import { IConnString, IEnvironment, ILogin, ISettings } from "../app.interfaces";
import { ICanComponentDeactivate } from "../guards";
import { AppService, MessageService, UtilService } from "../services";

@Component({
    templateUrl: "./settings.template.html",
    styleUrls: ["./settings.style.scss"],
})
export class SettingsComponent implements OnInit, ICanComponentDeactivate {
    public form: FormGroup;
    public settings: ISettings;
    public saving: boolean = false;
    get logins(): FormArray {
        return this.form.get("logins") as FormArray;
    }
    get connStrings(): FormArray {
        return this.form.get("connStrings") as FormArray;
    }
    get environments(): FormArray {
        return this.form.get("environments") as FormArray;
    }
    constructor(
        private messageService: MessageService, private appService: AppService, private formBuilder: FormBuilder,
        private utilService: UtilService,
    ) { this.createForm(); }
    public async ngOnInit() {
        try {
            this.settings = await this.appService.getSettings();
            this.updateForm();
        } catch (e) {
            this.messageService.error(`Failed to load: ${e.toString()}`);
            console.error(e);
        }
    }
    public async canDeactivate() {
        return this.form.dirty ? await this.utilService.confirmNavigation() : true;
    }
    public back() {
        this.utilService.back();
    }
    public updateForm() {
        this.form.reset();

        const loginGroups = this.settings.Logins.map(login => this.formBuilder.group({
            id: login.ID,
            username: [login.Username, Validators.required],
            password: [login.Password, Validators.required],
            domain: login.Domain,
        }));
        while (this.logins.length) {
            this.logins.removeAt(0);
        }
        for (const group of loginGroups) {
            this.logins.push(group);
        }

        const connStringGroups = this.settings.ConnStrings.map(connString => this.formBuilder.group({
            id: connString.ID,
            name: [connString.Name, Validators.required],
            environment: [connString.EnvironmentID, Validators.required],
            connString: [connString.Value, Validators.required],
        }));
        while (this.connStrings.length) {
            this.connStrings.removeAt(0);
        }
        for (const group of connStringGroups) {
            this.connStrings.push(group);
        }

        const environmentGroups = this.settings.Environments.map(environment => this.formBuilder.group({
            id: environment.ID,
            name: [environment.Name, Validators.required],
        }));
        while (this.environments.length) {
            this.environments.removeAt(0);
        }
        for (const group of environmentGroups) {
            this.environments.push(group);
        }

        this.form.markAsPristine();
    }
    public async save() {
        try {
            if (this.form.invalid) { return; }
            this.saving = true;
            this.settings = await this.appService.setSettings(this.modelToSettings());
            this.updateForm();
            this.messageService.success("Saved settings");
        } catch (e) {
            this.messageService.error(`Failed to save: ${e.toString()}`);
        } finally {
            this.saving = false;
        }
    }
    public addLogin() {
        this.logins.push(this.formBuilder.group({
            username: ["", Validators.required],
            password: ["", Validators.required],
            domain: "",
        }));
        this.form.markAsDirty();
    }
    public deleteLogin(index: number) {
        this.logins.removeAt(index);
        this.form.markAsDirty();
    }
    public addConnString() {
        this.connStrings.push(this.formBuilder.group({
            name: ["", Validators.required],
            environment: [undefined, Validators.required],
            connString: ["", Validators.required],
        }));
        this.form.markAsDirty();
    }
    public deleteConnString(index: number) {
        this.connStrings.removeAt(index);
        this.form.markAsDirty();
    }
    public addEnvironment() {
        this.environments.push(this.formBuilder.group({
            name: ["", Validators.required],
        }));
        this.form.markAsDirty();
    }
    public deleteEnvironment(index: number) {
        this.environments.removeAt(index);
        this.form.markAsDirty();
    }
    private modelToSettings() {
        const model = this.form.value;
        const settings: ISettings = {
            Logins: model.logins.map((login: any): ILogin => ({
                ID: login.id,
                Username: login.username,
                Password: login.password,
                Domain: login.domain,
            })),
            ConnStrings: model.connStrings.map((connString: any): IConnString => ({
                ID: connString.id,
                Name: connString.name,
                EnvironmentID: connString.environment,
                Value: connString.connString,
            })),
            Environments: model.environments.map((environment: any): IEnvironment => ({
                ID: environment.id,
                Name: environment.name,
            })),
        };
        return settings;
    }
    private createForm() {
        this.form = this.formBuilder.group({
            logins: this.formBuilder.array([]),
            connStrings: this.formBuilder.array([]),
            environments: this.formBuilder.array([]),
        });
    }
}
