import { Component, OnInit } from "@angular/core";
import { AbstractControl, FormArray, FormBuilder, FormGroup, ValidationErrors, ValidatorFn, Validators } from "@angular/forms";
import { ContactType } from "../app.enums";
import { ICheckGroup, IConnString, IContact, IContactType, IEnvironment, ILogin, ISettings } from "../app.interfaces";
import { ICanComponentDeactivate } from "../guards";
import { AppService, MessageService, UtilService } from "../services";

const EMAIL_REGEX = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
const PHONE_REGEX = /^[\+]?[(]?[0-9]{3}[)]?[-\s\.]?[0-9]{3}[-\s\.]?[0-9]{4,6}$/i;

export const contactValidator: ValidatorFn = async (control: AbstractControl): Promise<ValidationErrors | null> => {
    let valid = true;
    if (!control.parent) { return null; }
    switch (control.parent.value.type as ContactType) {
        case ContactType.Email:
            if (!EMAIL_REGEX.test(control.value)) {
                valid = false;
            }
            break;
        case ContactType.Phone:
            if (!PHONE_REGEX.test(control.value)) {
                valid = false;
            }
            break;
    }
    if (!valid) {
        return { invalid: true };
    } else {
        return null;
    }
};

@Component({
    templateUrl: "./settings.template.html",
    styleUrls: ["./settings.style.scss"],
})
export class SettingsComponent implements OnInit, ICanComponentDeactivate {
    public form: FormGroup;
    public settings: ISettings;
    public contactTypes: IContactType[];
    public saving = false;
    get logins(): FormArray {
        return this.form.get("logins") as FormArray;
    }
    get connStrings(): FormArray {
        return this.form.get("connStrings") as FormArray;
    }
    get environments(): FormArray {
        return this.form.get("environments") as FormArray;
    }
    get contacts(): FormArray {
        return this.form.get("contacts") as FormArray;
    }
    get checkGroups(): FormArray {
        return this.form.get("checkGroups") as FormArray;
    }
    constructor(
        private messageService: MessageService, private appService: AppService, private formBuilder: FormBuilder,
        private utilService: UtilService,
    ) { this.createForm(); }
    public async ngOnInit() {
        try {
            this.contactTypes = await this.appService.getContactTypes();
            this.settings = await this.appService.getSettings();
            this.updateForm();
        } catch (e) {
            this.messageService.error("Failed to load", e.toString());
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

        const global: FormGroup = this.form.controls.global as FormGroup;
        global.reset({
            authenticationGroup: this.settings.Global.AuthenticationGroup,
            slackToken: this.settings.Global.SlackToken,
            cleanupSchedule: this.settings.Global.CleanupSchedule,
            resultAggregateDays: this.settings.Global.ResultAggregateDays,
            resultRetentionMonths: this.settings.Global.ResultRetentionMonths,
            url: this.settings.Global.Url,
            loginExpireAfterDays: this.settings.Global.LoginExpireAfterDays,
            timeZoneId: this.settings.Global.TimeZoneId,
            countryCode: this.settings.Global.CountryCode,
        });

        const email = this.settings.Global.Email;
        if (email) {
            global.controls.email.reset({
                from: email.From,
                server: email.Server,
                port: email.Port,
                username: email.Username,
                password: email.Password,
                tls: email.TLS,
            });
        }

        const clickatell = this.settings.Global.Clickatell;
        if (clickatell) {
            global.controls.clickatell.reset({
                apiKey: clickatell.ApiKey,
                apiUrl: clickatell.ApiUrl,
                from: clickatell.From,
            });
        }

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

        const contactGroups = this.settings.Contacts.map(contact => this.formBuilder.group({
            id: contact.ID,
            type: [contact.TypeID, Validators.required],
            name: [contact.Name, Validators.required],
            value: [contact.Value, Validators.required, contactValidator],
        }));
        while (this.contacts.length) {
            this.contacts.removeAt(0);
        }
        for (const group of contactGroups) {
            this.contacts.push(group);
        }

        const checkGroupGroups = this.settings.CheckGroups.map(group => this.formBuilder.group({
            id: group.ID,
            name: [group.Name, Validators.required],
        }));
        while (this.checkGroups.length) {
            this.checkGroups.removeAt(0);
        }
        for (const group of checkGroupGroups) {
            this.checkGroups.push(group);
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
            this.messageService.error("Failed to save", e.toString());
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
    public addContact() {
        this.contacts.push(this.formBuilder.group({
            type: [undefined, Validators.required],
            name: ["", Validators.required],
            value: ["", Validators.required, contactValidator],
        }));
        this.form.markAsDirty();
    }
    public deleteContact(index: number) {
        this.contacts.removeAt(index);
        this.form.markAsDirty();
    }
    public addCheckGroup() {
        this.checkGroups.push(this.formBuilder.group({
            name: ["", Validators.required],
        }));
        this.form.markAsDirty();
    }
    public deleteCheckGroup(index: number) {
        this.checkGroups.removeAt(index);
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
            Contacts: model.contacts.map((contact: any): IContact => ({
                ID: contact.id,
                TypeID: contact.type,
                Name: contact.name,
                Value: contact.value,
            })),
            CheckGroups: model.checkGroups.map((checkGroup: any): ICheckGroup => ({
                ID: checkGroup.id,
                Name: checkGroup.name,
            })),
            Global: {
                Email: {
                    From: model.global.email.from,
                    Server: model.global.email.server,
                    Port: model.global.email.port,
                    Username: model.global.email.username,
                    Password: model.global.email.password,
                    TLS: model.global.email.tls,
                },
                Clickatell: {
                    ApiKey: model.global.clickatell.apiKey,
                    ApiUrl: model.global.clickatell.apiUrl,
                    From: model.global.clickatell.from,
                },
                AuthenticationGroup: model.global.authenticationGroup,
                SlackToken: model.global.slackToken,
                CleanupSchedule: model.global.cleanupSchedule,
                LoginExpireAfterDays: model.global.loginExpireAfterDays,
                ResultAggregateDays: model.global.resultAggregateDays,
                ResultRetentionMonths: model.global.resultRetentionMonths,
                Url: model.global.url,
                TimeZoneId: model.global.timeZoneId,
                CountryCode: model.global.countryCode,
            },
        };
        return settings;
    }
    private createForm() {
        this.form = this.formBuilder.group({
            logins: this.formBuilder.array([]),
            connStrings: this.formBuilder.array([]),
            environments: this.formBuilder.array([]),
            contacts: this.formBuilder.array([]),
            checkGroups: this.formBuilder.array([]),
            global: this.formBuilder.group({
                email: this.formBuilder.group({
                    from: [undefined],
                    server: [undefined],
                    port: [undefined],
                    username: [undefined],
                    password: [undefined],
                    tls: [undefined],
                }),
                clickatell: this.formBuilder.group({
                    apiKey: [undefined],
                    apiUrl: [undefined],
                    from: [undefined],
                }),
                authenticationGroup: [undefined],
                slackToken: [undefined],
                cleanupSchedule: [undefined],
                resultAggregateDays: [undefined],
                resultRetentionMonths: [undefined],
                url: [undefined],
                loginExpireAfterDays: [Validators.required, Validators.min(1)],
                timeZoneId: [Validators.required],
                countryCode: [Validators.required],
            }),
        });
    }
}
