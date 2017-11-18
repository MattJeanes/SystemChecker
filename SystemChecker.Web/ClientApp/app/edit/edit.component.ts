import { Location } from "@angular/common";
import { Component, OnInit } from "@angular/core";
import { AbstractControl, FormArray, FormBuilder, FormGroup, ValidationErrors, ValidatorFn, Validators } from "@angular/forms";
import { ActivatedRoute, Router } from "@angular/router";

import { ICheckDetail, ICheckSchedule, ICheckType, ICheckTypeOption, ISettings } from "../app.interfaces";
import { RunCheckComponent } from "../components";
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
    public updateForm() {
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
