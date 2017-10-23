import { Location } from "@angular/common";
import { Component, OnInit } from "@angular/core";
import { AbstractControl, FormArray, FormBuilder, FormGroup, ValidationErrors, ValidatorFn, Validators } from "@angular/forms";
import { ActivatedRoute, Router } from "@angular/router";
import * as later from "later";

import { ICheckDetail, ICheckSchedule, ICheckType, ICheckTypeOption, ISettings } from "../app.interfaces";
import { RunCheckComponent } from "../components";
import { AppService, MessageService, UtilService } from "../services";

const cronValidRegex = /^(\*|((\*\/)?[1-5]?[0-9])) (\*|((\*\/)?[1-5]?[0-9])) (\*|((\*\/)?(1?[0-9]|2[0-3]))) (\*|((\*\/)?([1-9]|[12][0-9]|3[0-1]))) (\*|((\*\/)?([1-9]|1[0-2]))) (\*|((\*\/)?[0-6]))$/;

export function cronValidator(): ValidatorFn {
    return async (control: AbstractControl): Promise<ValidationErrors | null> => {
        if (!cronValidRegex.test(control.value)) {
            return { invalidCron: { value: control.value } };
        } else {
            return null;
        }
    };
}

@Component({
    templateUrl: "./edit.template.html",
    styleUrls: ["./edit.style.scss"],
})
export class EditComponent implements OnInit {
    public check: ICheckDetail = this.getNewCheck(true);
    public types: ICheckType[] = [];
    public settings: ISettings;
    public form: FormGroup;
    public saving: boolean = false;
    get schedules(): FormArray {
        return this.form.get("schedules") as FormArray;
    }
    get options(): FormArray {
        return this.form.get("options") as FormArray;
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
            this.updateForm();
            this.updateUrl();
        } catch (e) {
            this.messageService.error(`Failed to load: ${e.toString()}`);
            console.error(e);
        }
    }
    public async save() {
        try {
            if (this.form.invalid) { return; }
            this.saving = true;
            this.prepareForSave();
            this.check = await this.appService.edit(this.check);
            this.updateForm();
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
            expression: ["", Validators.required, cronValidator()],
            active: true,
        }));
        this.form.markAsDirty();
    }
    public deleteSchedule(index: number) {
        this.schedules.removeAt(index);
        this.form.markAsDirty();
    }
    public validateExpression(index: number) {
        try {
            const schedule = this.schedules.get(index.toString());
            if (!schedule) { return "Invalid schedule"; }
            const expression = schedule.get("expression")!.value;
            if (!expression || !cronValidRegex.test(expression)) { return "Invalid cron expression"; }
            const data = later.parse.cron(expression, true);
            const next = later.schedule(data).next(5).map(x => x.toLocaleString()).join("<br />");
            return next;
        } catch (e) {
            return e.toString();
        }
    }
    public updateForm() {
        this.form.reset({
            name: this.check.Name,
            type: this.check.TypeID ? this.check.TypeID : null,
            active: this.check.Active,
        });

        const scheduleGroups = this.check.Schedules.map(schedule => this.formBuilder.group({
            id: schedule.ID,
            expression: [schedule.Expression ? schedule.Expression : "", Validators.required, cronValidator()],
            active: schedule.Active,
        }));

        // Workaround, using `this.form.setControl("schedules", this.formBuilder.array([]));` seems to break stuff
        while (this.schedules.length) {
            this.schedules.removeAt(0);
        }
        for (const group of scheduleGroups) {
            this.schedules.push(group);
        }

        const type = this.types.find(x => x.ID === this.check.TypeID);
        if (type) {
            this.changeType(type);
        }
        this.form.markAsPristine();
    }
    public changeType(type: ICheckType) {
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
    }
    public onTypeChange() {
        const formType = this.form.get("type")!.value as number;
        const type = this.types.find(x => x.ID === formType);
        if (type) {
            this.changeType(type);
        }
    }
    public run() {
        this.appService.run(RunCheckComponent, this.check);
    }
    private prepareForSave() {
        const model = this.form.value;
        this.check.Name = model.name;
        this.check.Active = model.active;
        this.check.TypeID = model.type;
        this.check.Schedules = model.schedules.map((schedule: any): ICheckSchedule => ({
            ID: schedule.id,
            Expression: schedule.expression,
            Active: schedule.active,
        }));
        this.check.Data.TypeOptions = {};
        model.options.forEach((option: { value: any, option: ICheckTypeOption }) => this.check.Data.TypeOptions[option.option.ID] = option.value);
    }
    private getNewCheck(loading?: boolean): ICheckDetail {
        return {
            ID: 0,
            Active: true,
            Name: loading ? "Loading.." : "New Check",
            Schedules: [],
            Data: { TypeOptions: {} },
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
        });
    }
}
