import { CommonModule, registerLocaleData } from "@angular/common";
import { HTTP_INTERCEPTORS, HttpClientModule } from "@angular/common/http";
import localeGB from "@angular/common/locales/en-GB";
import { LOCALE_ID, NgModule } from "@angular/core";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { HttpModule } from "@angular/http";
import { BrowserModule } from "@angular/platform-browser";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { RouterModule, Routes } from "@angular/router";
import { JwtModule } from "@auth0/angular-jwt";
import * as store from "store";

import { Autosize } from "./directives";

import { OptionComponent, RunCheckComponent } from "./components";

import { AuthGuard, CanDeactivateGuard } from "./guards";

import { AppComponent } from "./app.component";
import { DashboardComponent } from "./dashboard/dashboard.component";
import { DetailsComponent } from "./details/details.component";
import { EditComponent } from "./edit/edit.component";
import { PageNotFoundComponent } from "./errors/not-found.component";
import { InitComponent } from "./init/init.component";
import { LoginComponent } from "./login/login.component";
import { SettingsComponent } from "./settings/settings.component";
import { UserComponent } from "./user/user.component";

import {
    AppService,
    AuthInterceptor,
    MessageService,
    PageService,
    UtilService,
} from "./services";

import {
    MAT_DATE_LOCALE,
    MatButtonModule,
    MatCardModule,
    MatCheckboxModule,
    MatDatepickerModule,
    MatIconModule,
    MatInputModule,
    MatNativeDateModule,
    MatPaginatorModule,
    MatSelectModule,
    MatSnackBarModule,
    MatSortModule,
    MatTableModule,
    MatTabsModule,
    MatTooltipModule,
} from "@angular/material";

import {
    CovalentDialogsModule,
    CovalentLoadingModule,
} from "@covalent/core";

import {
    NgxChartsModule,
} from "@swimlane/ngx-charts";

registerLocaleData(localeGB);

export function JwtTokenGetter(): string {
    return store.get("token");
}

const routes: Routes = [
    { path: "", redirectTo: "dashboard", pathMatch: "full" },
    { path: "dashboard", component: DashboardComponent, data: { title: "Dashboard", noDashboardLink: true } },
    { path: "edit", component: EditComponent, data: { title: "New Check" } },
    { path: "edit/:id", component: EditComponent, data: { title: "Edit Check" } },
    { path: "edit/:id/:copy", component: EditComponent, data: { title: "Copy Check" } },
    { path: "settings", component: SettingsComponent, data: { title: "Settings" } },
    { path: "details/:id", component: DetailsComponent, data: { title: "Details" } },
    { path: "login", component: LoginComponent, data: { title: "Login", noDashboardLink: true } },
    { path: "user", component: UserComponent, data: { title: "User" } },
    { path: "init", component: InitComponent, data: { title: "SystemChecker Init", noDashboardLink: true } },
    { path: "**", component: PageNotFoundComponent, data: { title: "Not Found" } },
];

routes.forEach(x => {
    if (!x.canDeactivate) {
        x.canDeactivate = [];
    }
    x.canDeactivate.push(CanDeactivateGuard);

    if (x.path === "login" || x.path === "init") { return; }
    if (!x.canActivate) {
        x.canActivate = [];
    }
    x.canActivate.push(AuthGuard);
});

@NgModule({
    imports: [
        CommonModule,
        BrowserModule,
        BrowserAnimationsModule,
        HttpModule,
        FormsModule,
        RouterModule.forRoot(routes),
        JwtModule.forRoot({
            config: {
                tokenGetter: JwtTokenGetter,
            },
        }),
        MatButtonModule,
        MatCardModule,
        MatInputModule,
        MatCheckboxModule,
        MatTabsModule,
        MatSelectModule,
        ReactiveFormsModule,
        MatDatepickerModule,
        MatNativeDateModule,
        CovalentDialogsModule,
        NgxChartsModule,
        MatTooltipModule,
        CovalentLoadingModule,
        MatIconModule,
        HttpClientModule,
        MatSnackBarModule,
        MatTableModule,
        MatSortModule,
        MatPaginatorModule,
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
        LoginComponent,
        UserComponent,
        InitComponent,
        Autosize,
    ],
    exports: [
        RouterModule,
    ],
    providers: [
        AppService,
        MessageService,
        UtilService,
        PageService,
        CanDeactivateGuard,
        AuthGuard,
        { provide: LOCALE_ID, useValue: "en-GB" },
        { provide: MAT_DATE_LOCALE, useValue: "en-GB" },
        { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true },
    ],
    entryComponents: [
        RunCheckComponent,
    ],
    bootstrap: [AppComponent],
})
export class AppModule { }
