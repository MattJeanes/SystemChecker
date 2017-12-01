import { CommonModule, registerLocaleData } from "@angular/common";
import localeGB from "@angular/common/locales/en-GB";
import { LOCALE_ID, NgModule } from "@angular/core";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { HttpModule } from "@angular/http";
import { BrowserModule } from "@angular/platform-browser";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { RouterModule, Routes } from "@angular/router";

import { Autosize } from "./directives";

import { OptionComponent, RunCheckComponent } from "./components";

import { AppComponent } from "./app.component";
import { DashboardComponent } from "./dashboard/dashboard.component";
import { DetailsComponent } from "./details/details.component";
import { EditComponent } from "./edit/edit.component";
import { PageNotFoundComponent } from "./errors/not-found.component";
import { SettingsComponent } from "./settings/settings.component";

import {
    AppService,
    MessageService,
    UtilService,
} from "./services";

import {
    MAT_DATE_LOCALE,
    MatButtonModule,
    MatCardModule,
    MatCheckboxModule,
    MatDatepickerModule,
    MatInputModule,
    MatNativeDateModule,
    MatSelectModule,
    MatTabsModule,
    MatTooltipModule,
} from "@angular/material";

import {
    DataTableModule,
    DropdownModule,
    GrowlModule,
} from "primeng/primeng";

import {
    CovalentDialogsModule,
} from "@covalent/core";

import {
    NgxChartsModule,
} from "@swimlane/ngx-charts";

registerLocaleData(localeGB);

const routes: Routes = [
    { path: "", redirectTo: "dashboard", pathMatch: "full" },
    { path: "dashboard", component: DashboardComponent },
    { path: "edit", component: EditComponent },
    { path: "edit/:id", component: EditComponent },
    { path: "edit/:id/:copy", component: EditComponent },
    { path: "settings", component: SettingsComponent },
    { path: "details/:id", component: DetailsComponent },
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
        NgxChartsModule,
        MatTooltipModule,
    ],
    declarations: [
        AppComponent,
        DashboardComponent,
        EditComponent,
        OptionComponent,
        SettingsComponent,
        RunCheckComponent,
        PageNotFoundComponent,
        DetailsComponent,
        Autosize,
    ],
    exports: [
        RouterModule,
    ],
    providers: [
        AppService,
        MessageService,
        UtilService,
        { provide: LOCALE_ID, useValue: "en-GB" },
        { provide: MAT_DATE_LOCALE, useValue: "en-GB" },
    ],
    entryComponents: [
        RunCheckComponent,
    ],
    bootstrap: [AppComponent],
})
export class AppModule { }
