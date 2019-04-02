import { Component, OnInit } from "@angular/core";
import { FormArray, FormBuilder, FormGroup, Validators } from "@angular/forms";

import { IApiKey, IUser } from "../app.interfaces";
import { AppService, UtilService } from "../services";

@Component({
    templateUrl: "./user.template.html",
    styleUrls: ["./user.style.scss"],
})
export class UserComponent implements OnInit {
    public saving = false;
    public form: FormGroup;

    private user: IUser;

    get apiKeys() {
        return this.form.get("apiKeys") as FormArray;
    }

    constructor(private appService: AppService, private utilService: UtilService, private formBuilder: FormBuilder) { this.createForm(); }

    public async ngOnInit() {
        try {
            this.user = await this.appService.getUser();
            this.updateForm();
        } catch (e) {
            console.error(e);
            this.utilService.alert("Failed to load user", e.toString());
        }
    }

    public back() {
        this.utilService.back();
    }

    public logout() {
        this.appService.logout();
    }

    public addApiKey() {
        this.apiKeys.push(this.formBuilder.group({
            name: ["", Validators.required],
            key: ["", Validators.required],
        }));
        this.form.markAsDirty();
    }
    public deleteApiKey(index: number) {
        this.apiKeys.removeAt(index);
        this.form.markAsDirty();
    }

    public async save() {
        try {
            this.saving = true;
            const user = this.modelToUser();
            this.user = await this.appService.editUser(user);
            this.updateForm();
        } catch (e) {
            this.utilService.alert("Failed to save user", e.toString());
        } finally {
            this.saving = false;
        }
    }

    public updateForm() {
        this.form.reset({
            id: this.user.ID,
            username: this.user.Username,
            password: this.user.Password,
            isWindowsUser: this.user.IsWindowsUser,
        });

        const apiKeyGroups = this.user.ApiKeys.map(apiKey => this.formBuilder.group({
            id: apiKey.ID,
            name: [apiKey.Name, Validators.required],
            key: [apiKey.Key, Validators.required],
        }));

        while (this.apiKeys.length) {
            this.apiKeys.removeAt(0);
        }
        for (const group of apiKeyGroups) {
            this.apiKeys.push(group);
        }
    }

    private modelToUser() {
        const model = this.form.value;
        const user: IUser = {
            ID: model.id,
            Username: model.username,
            Password: model.password,
            IsWindowsUser: model.isWindowsUser,
            ApiKeys: model.apiKeys.map((apiKey: any): IApiKey => ({
                ID: apiKey.id,
                UserID: this.user.ID,
                Name: apiKey.name,
                Key: apiKey.key,
            })),
        };
        return user;
    }

    private createForm() {
        this.form = this.formBuilder.group({
            id: [undefined, Validators.required],
            username: [undefined, Validators.required],
            password: [undefined],
            isWindowsUser: [undefined, Validators.required],
            apiKeys: this.formBuilder.array([]),
        });
    }
}
