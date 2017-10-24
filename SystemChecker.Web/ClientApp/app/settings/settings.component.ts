import { Component, OnInit } from "@angular/core";
import { FormArray, FormBuilder, FormGroup, Validators } from "@angular/forms";
import { IConnString, ILogin, ISettings } from "../app.interfaces";
import { AppService, MessageService } from "../services";

@Component({
    templateUrl: "./settings.template.html",
    styleUrls: ["./settings.style.scss"],
})
export class SettingsComponent implements OnInit {
    public form: FormGroup;
    public settings: ISettings;
    public saving: boolean = false;
    get logins(): FormArray {
        return this.form.get("logins") as FormArray;
    }
    get connStrings(): FormArray {
        return this.form.get("connStrings") as FormArray;
    }
    constructor(
        private messageService: MessageService, private appService: AppService, private formBuilder: FormBuilder,
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
            connString: [connString.Value, Validators.required],
        }));
        while (this.connStrings.length) {
            this.connStrings.removeAt(0);
        }
        for (const group of connStringGroups) {
            this.connStrings.push(group);
        }

        this.form.markAsPristine();
    }
    public async save() {
        try {
            if (this.form.invalid) { return; }
            this.saving = true;
            this.prepareForSave();
            this.settings = await this.appService.setSettings(this.settings);
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
            connString: ["", Validators.required],
        }));
        this.form.markAsDirty();
    }
    public deleteConnString(index: number) {
        this.connStrings.removeAt(index);
        this.form.markAsDirty();
    }
    private prepareForSave() {
        const model = this.form.value;
        this.settings.Logins = model.logins.map((login: any): ILogin => ({
            ID: login.id,
            Username: login.username,
            Password: login.password,
            Domain: login.domain,
        }));
        this.settings.ConnStrings = model.connStrings.map((connString: any): IConnString => ({
            ID: connString.id,
            Name: connString.name,
            Value: connString.connString,
        }));
    }
    private createForm() {
        this.form = this.formBuilder.group({
            logins: this.formBuilder.array([]),
            connStrings: this.formBuilder.array([]),
        });
    }
}
