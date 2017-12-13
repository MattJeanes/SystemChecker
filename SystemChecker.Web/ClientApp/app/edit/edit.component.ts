import { Location } from "@angular/common";
import { Component, OnInit } from "@angular/core";
import { AbstractControl, FormArray, FormBuilder, FormGroup, ValidationErrors, ValidatorFn, Validators } from "@angular/forms";
import { ActivatedRoute, Router } from "@angular/router";

import { ICheckDetail, ICheckNotification, ICheckNotificationType, ICheckSchedule, ICheckType, IOption, ISettings, ISubCheck, ISubCheckType } from "../app.interfaces";
import { RunCheckComponent } from "../components";
import { ICanComponentDeactivate } from "../guards";
import { AppService, MessageService, UtilService } from "../services";

export function cronValidator(appService: AppService): ValidatorFn {
    return async (control: AbstractControl): Promise<ValidationErrors | null> => {
        let valid = true;
        try {
            const validate = await appService.validateCronExpression(control.value, true);
            if (!validate.valid) {
                valid = false;
            }
        } catch (e) {
            valid = false;
        }
        if (!valid) {
            return { invalidCron: { value: control.value } };
        } else {
            return null;
        }
    };
}

export function oneFilledOutValidator(): ValidatorFn {
    return (group: FormGroup): ValidationErrors | null => {
        const fields = [];
        for (const field in group.controls) {
            if (group.controls.hasOwnProperty(field)) {
                fields.push(group.get(field)!.value);
            }
        }
        const result = fields.filter(field => field !== null);
        const valid = result.length !== 0;
        return valid ? null : {
            oneFilledOut: true,
        };
    };
}

@Component({
    templateUrl: "./edit.template.html",
    styleUrls: ["./edit.style.scss"],
})
export class EditComponent implements OnInit, ICanComponentDeactivate {
    public check: ICheckDetail = this.getNewCheck(true);
    public types: ICheckType[] = [];
    public subCheckTypes: ISubCheckType[] = [];
    public notificationTypes: ICheckNotificationType[] = [];
    public settings: ISettings;
    public form: FormGroup;
    public saving: boolean = false;
    get schedules(): FormArray {
        return this.form.get("schedules") as FormArray;
    }
    get options(): FormArray {
        return this.form.get("options") as FormArray;
    }
    get subChecks(): FormArray {
        return this.form.get("subChecks") as FormArray;
    }
    get notifications(): FormArray {
        return this.form.get("notifications") as FormArray;
    }
    constructor(
        private appService: AppService, private activatedRoute: ActivatedRoute,
        private location: Location, private messageService: MessageService, private utilService: UtilService,
        private router: Router, private formBuilder: FormBuilder) { this.createForm(); }
    public async ngOnInit() {
        try {
            const params = await this.activatedRoute.params.first().toPromise();
            const id = parseInt(params.id);
            if (id) {
                this.check = await this.appService.getDetails(id);
                if (params.copy) {
                    this.check.ID = 0;
                    this.check.Name += " - copy";
                }
            } else {
                this.check = this.getNewCheck();
            }
            this.types = await this.appService.getTypes();
            this.settings = await this.appService.getSettings();
            this.notificationTypes = await this.appService.getCheckNotificationTypes();
            await this.updateForm();
            this.updateUrl();
        } catch (e) {
            this.messageService.error(`Failed to load: ${e.toString()}`);
            console.error(e);
        }
    }
    public async canDeactivate() {
        return this.form.dirty ? await this.utilService.confirmNavigation() : true;
    }
    public async save() {
        try {
            if (this.form.invalid) { return; }
            this.saving = true;
            this.check = await this.appService.edit(this.modelToCheck());
            await this.updateForm();
            this.updateUrl();
            this.messageService.success("Saved check");
        } catch (e) {
            this.messageService.error(`Failed to save: ${e.toString()}`);
        } finally {
            this.saving = false;
        }
    }
    public async delete() {
        try {
            const confirm = await this.utilService.confirm(`Delete ${this.check.Name}`, `Are you sure you want to delete the '${this.check.Name}' check? You cannot undo this action.`);
            if (confirm && this.check.ID) {
                await this.appService.delete(this.check.ID);
                this.messageService.success(`Deleted ${this.check.Name}`);
                this.router.navigate(["/dashboard"]);
            }
        } catch (e) {
            this.messageService.error(`Failed to delete: ${e.toString()}`);
        }
    }
    public addSchedule() {
        this.schedules.push(this.formBuilder.group({
            expression: ["", Validators.required, cronValidator(this.appService)],
            active: true,
            next: { value: "", disabled: true },
        }));
        this.form.markAsDirty();
    }
    public deleteSchedule(index: number) {
        this.schedules.removeAt(index);
        this.form.markAsDirty();
    }
    public addSubCheck() {
        this.subChecks.push(this.formBuilder.group({
            type: [undefined, Validators.required],
            active: true,
            options: this.formBuilder.array([]),
        }));
        this.form.markAsDirty();
    }
    public deleteSubCheck(index: number) {
        this.subChecks.removeAt(index);
        this.form.markAsDirty();
    }
    public addNotification() {
        this.notifications.push(this.formBuilder.group({
            type: [undefined, Validators.required],
            active: true,
            options: this.formBuilder.array([]),
            conditions: this.formBuilder.group({
                failCount: [undefined],
                failMinutes: [undefined],
            }, { validator: oneFilledOutValidator() }),
        }));
        this.form.markAsDirty();
    }
    public deleteNotification(index: number) {
        this.notifications.removeAt(index);
        this.form.markAsDirty();
    }
    public async validateExpression(index: number | AbstractControl) {
        let nextField: AbstractControl | null = null;
        let value: string = "";
        try {
            const schedule = typeof (index) === "number" ? this.schedules.get(index.toString()) : index;
            if (!schedule) { throw new Error("Invalid schedule"); }
            const expression = schedule.get("expression")!.value;
            if (!expression) { throw new Error("Expression is required"); }
            nextField = schedule.get("next")!;
            const validate = await this.appService.validateCronExpression(expression);
            if (validate.valid && validate.next) {
                value = validate.next;
            } else if (validate.error) {
                value = validate.error;
            } else {
                value = "Unknown error";
            }
        } catch (e) {
            value = e.toString();
        }
        if (nextField) {
            nextField.setValue(value);
        }
    }
    public async updateForm() {
        this.form.reset({
            name: this.check.Name,
            type: this.check.TypeID ? this.check.TypeID : null,
            active: this.check.Active,
        });

        const scheduleGroups = this.check.Schedules.map(schedule => this.formBuilder.group({
            id: schedule.ID,
            expression: [schedule.Expression ? schedule.Expression : "", Validators.required, cronValidator(this.appService)],
            active: schedule.Active,
            next: { value: "", disabled: true },
        }));

        // Workaround, using `this.form.setControl("schedules", this.formBuilder.array([]));` seems to break stuff
        while (this.schedules.length) {
            this.schedules.removeAt(0);
        }
        for (const group of scheduleGroups) {
            this.schedules.push(group);
        }
        for (const control of this.schedules.controls) {
            this.validateExpression(control);
        }

        const subCheckGroups = this.check.SubChecks.map(subCheck => this.formBuilder.group({
            id: subCheck.ID,
            type: [subCheck.TypeID, Validators.required],
            active: subCheck.Active,
            options: this.formBuilder.array([]),
        }));

        while (this.subChecks.length) {
            this.subChecks.removeAt(0);
        }
        for (const group of subCheckGroups) {
            this.subChecks.push(group);
        }

        const notificationGroups = this.check.Notifications.map(notification => this.formBuilder.group({
            id: notification.ID,
            type: [notification.TypeID, Validators.required],
            active: notification.Active,
            options: this.formBuilder.array([]),
            conditions: this.formBuilder.group({
                failCount: [notification.FailCount],
                failMinutes: [notification.FailMinutes],
            }, { validator: oneFilledOutValidator() }),
        }));

        while (this.notifications.length) {
            this.notifications.removeAt(0);
        }
        for (const group of notificationGroups) {
            this.notifications.push(group);
        }

        const type = this.types.find(x => x.ID === this.check.TypeID);
        if (type) {
            await this.changeType(type);
        }

        for (const key in this.check.SubChecks) {
            if (!this.check.SubChecks.hasOwnProperty(key)) { continue; }
            const subCheck = this.check.SubChecks[key];
            const subCheckType = this.subCheckTypes.find(x => x.ID === subCheck.TypeID);
            if (subCheckType) {
                const control = this.subChecks.controls[key] as FormGroup;
                this.changeSubCheckType(control.controls.options as FormArray, subCheckType, subCheck);
            }
        }

        for (const key in this.check.Notifications) {
            if (!this.check.Notifications.hasOwnProperty(key)) { continue; }
            const notification = this.check.Notifications[key];
            const notificationType = this.notificationTypes.find(x => x.ID === notification.TypeID);
            if (notificationType) {
                const control = this.notifications.controls[key] as FormGroup;
                this.changeNotificationType(control.controls.options as FormArray, notificationType, notification);
            }
        }

        if (this.check.ID > 0 && this.utilService.equals(this.check, this.modelToCheck())) {
            this.form.markAsPristine();
        } else {
            this.form.markAsDirty();
        }
    }
    public async changeType(type: ICheckType) {
        try {
            const optionGroups = type.Options.map(option => {
                const value = [];
                const currentValue = this.check.Data.TypeOptions[option.ID];
                value.push(currentValue !== undefined ? currentValue : option.DefaultValue);
                if (option.IsRequired) {
                    value.push(Validators.required);
                }
                return this.formBuilder.group({ value, option });
            });
            while (this.options.length) {
                this.options.removeAt(0);
            }
            for (const group of optionGroups) {
                this.options.push(group);
            }
            this.subCheckTypes = await this.appService.getSubCheckTypes(type.ID);
        } catch (e) {
            this.messageService.error("Failed to change type", e.toString());
        }
    }
    public onTypeChange() {
        const formType = this.form.get("type")!.value as number;
        const type = this.types.find(x => x.ID === formType);
        if (type) {
            this.changeType(type);
        }
    }
    public changeSubCheckType(options: FormArray, type: ISubCheckType, subCheck?: ISubCheck) {
        try {
            const optionGroups = type.Options.map(option => {
                const value = [];
                const currentValue = subCheck ? subCheck.Options[option.ID] : undefined;
                value.push(currentValue !== undefined ? currentValue : option.DefaultValue);
                if (option.IsRequired) {
                    value.push(Validators.required);
                }
                return this.formBuilder.group({ value, option });
            });
            while (options.length) {
                options.removeAt(0);
            }
            for (const group of optionGroups) {
                options.push(group);
            }
        } catch (e) {
            this.messageService.error("Failed to change sub-check type", e.toString());
        }
    }
    public onSubCheckTypeChange(id: number) {
        const control = this.subChecks.controls[id] as FormGroup;
        const options = control.controls.options as FormArray;
        const subCheckTypeID = control.value.type;
        const subCheckType = this.subCheckTypes.find(x => x.ID === subCheckTypeID);
        if (subCheckType) {
            this.changeSubCheckType(options, subCheckType);
        }
    }
    public changeNotificationType(options: FormArray, type: ICheckNotificationType, notification?: ICheckNotification) {
        try {
            const optionGroups = type.Options.map(option => {
                const value = [];
                const currentValue = notification ? notification.Options[option.ID] : undefined;
                value.push(currentValue !== undefined ? currentValue : option.DefaultValue);
                if (option.IsRequired) {
                    value.push(Validators.required);
                }
                return this.formBuilder.group({ value, option });
            });
            while (options.length) {
                options.removeAt(0);
            }
            for (const group of optionGroups) {
                options.push(group);
            }
        } catch (e) {
            this.messageService.error("Failed to change notification type", e.toString());
        }
    }
    public onNotificationTypeChange(id: number) {
        const control = this.notifications.controls[id] as FormGroup;
        const options = control.controls.options as FormArray;
        const notificationTypeID = control.value.type;
        const notificationType = this.notificationTypes.find(x => x.ID === notificationTypeID);
        if (notificationType) {
            this.changeNotificationType(options, notificationType);
        }
    }
    public run() {
        this.appService.run(RunCheckComponent, this.check);
    }
    public track(index: number, option: { value: any, option: IOption }) {
        return option ? option.option : undefined;
    }
    public back() {
        this.utilService.back();
    }
    private modelToCheck() {
        const model = this.form.value;
        const check: ICheckDetail = {
            ID: this.check.ID,
            Name: model.name,
            Active: model.active,
            TypeID: model.type,
            Schedules: model.schedules.map((schedule: any): ICheckSchedule => ({
                ID: schedule.id,
                Expression: schedule.expression,
                Active: schedule.active,
            })),
            Data: {
                TypeOptions: {},
            },
            SubChecks: model.subChecks.map((subCheck: any): ISubCheck => ({
                ID: subCheck.id,
                TypeID: subCheck.type,
                CheckID: this.check.ID,
                Active: subCheck.active,
                Options: {},
            })),
            Notifications: model.notifications.map((notification: any): ICheckNotification => ({
                ID: notification.id,
                TypeID: notification.type,
                CheckID: this.check.ID,
                Active: notification.active,
                Options: {},
                FailCount: notification.conditions.failCount,
                FailMinutes: notification.conditions.failMinutes,
            })),
        };
        for (const notification of check.Notifications) {
            if (notification.FailCount === null) {
                delete notification.FailCount;
            }
            if (notification.FailMinutes === null) {
                delete notification.FailMinutes;
            }
        }
        model.options.forEach((option: { value: any, option: IOption }) => check.Data.TypeOptions[option.option.ID] = option.value);
        model.subChecks.forEach((subCheck: any, index: number) => {
            subCheck.options.forEach((option: { value: any, option: IOption }) => check.SubChecks[index].Options[option.option.ID] = option.value);
        });
        model.notifications.forEach((notification: any, index: number) => {
            notification.options.forEach((option: { value: any, option: IOption }) => check.Notifications[index].Options[option.option.ID] = option.value);
        });
        return check;
    }
    private getNewCheck(loading?: boolean): ICheckDetail {
        return {
            ID: 0,
            Active: true,
            Name: loading ? "Loading.." : "New Check",
            Schedules: [],
            Data: {
                TypeOptions: {},
            },
            SubChecks: [],
            Notifications: [],
        };
    }
    private updateUrl() {
        const newPath = `/edit/${this.check.ID}`;
        const currentPath = this.location.path();
        if (newPath !== currentPath) {
            this.location.replaceState(newPath);
        }
    }
    private createForm() {
        this.form = this.formBuilder.group({
            name: ["", Validators.required],
            type: [undefined, Validators.required],
            active: false,
            schedules: this.formBuilder.array([]),
            options: this.formBuilder.array([]),
            subChecks: this.formBuilder.array([]),
            notifications: this.formBuilder.array([]),
        });
    }
}
