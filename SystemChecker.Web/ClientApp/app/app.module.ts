import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { HttpModule } from "@angular/http";
import { BrowserModule } from "@angular/platform-browser";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { RouterModule, Routes } from "@angular/router";

import { OptionComponent, RunCheckComponent } from "./components";

import { AppComponent } from "./app.component";
import { DashboardComponent } from "./dashboard/dashboard.component";
import { EditComponent } from "./edit/edit.component";
import { PageNotFoundComponent } from "./errors/not-found.component";
import { SettingsComponent } from "./settings/settings.component";

import {
    AppService,
    MessageService,
    UtilService,
} from "./services";

import {
    MatButtonModule,
    MatCardModule,
    MatCheckboxModule,
    MatDatepickerModule,
    MatInputModule,
    MatNativeDateModule,
    MatSelectModule,
    MatTabsModule,
} from "@angular/material";

import {
    DataTableModule,
    DropdownModule,
    GrowlModule,
} from "primeng/primeng";

import {
    CovalentDialogsModule,
} from "@covalent/core";

import { Autosize } from "./directives";

const routes: Routes = [
    { path: "", redirectTo: "dashboard", pathMatch: "full" },
    { path: "dashboard", component: DashboardComponent },
    { path: "edit", component: EditComponent },
    { path: "edit/:id", component: EditComponent },
    { path: "edit/:id/:copy", component: EditComponent },
    { path: "settings", component: SettingsComponent },
    { path: "**", component: PageNotFoundComponent },
];

@NgModule({
    imports: [
        CommonModule,
        BrowserModule,
        BrowserAnimationsModule,
        HttpModule,
        FormsModule,
        RouterModule.forRoot(routes),
        DataTableModule,
        DropdownModule,
        MatButtonModule,
        MatCardModule,
        MatInputModule,
        MatCheckboxModule,
        MatTabsModule,
        MatSelectModule,
        ReactiveFormsModule,
        MatDatepickerModule,
        MatNativeDateModule,
        GrowlModule,
        CovalentDialogsModule,
    ],
    declarations: [
        AppComponent,
        DashboardComponent,
        EditComponent,
        OptionComponent,
        SettingsComponent,
        RunCheckComponent,
        PageNotFoundComponent,
        Autosize,
    ],
    exports: [
        RouterModule,
    ],
    providers: [
        AppService,
        MessageService,
        UtilService,
    ],
    entryComponents: [
        RunCheckComponent,
    ],
    bootstrap: [AppComponent],
})
export class AppModule { }
