import { CheckLogType, CheckResultStatus, OptionType } from "./app.enums";

export interface ICheck {
    ID: number;
    Name: string;
    Active: boolean;
    Description?: string;
    TypeID?: number;
    GroupID?: number;
    LastResultStatus?: CheckResultStatus;
    EnvironmentID?: number;
}

export interface ICheckSchedule {
    ID: number;
    Check?: ICheck;
    Expression: string;
    Active: boolean;
}

export interface ICheckData {
    TypeOptions: any;
}

export interface ICheckResult {
    ID: number;
    CheckID: number;
    DTS: string;
    Status: CheckResultStatus;
    TimeMS: number;
}

export interface ICheckDetail extends ICheck {
    Schedules: ICheckSchedule[];
    Data: ICheckData;
    SubChecks: ISubCheck[];
    Results?: ICheckResult[];
    Notifications: ICheckNotification[];
}

export interface ICheckType {
    ID: number;
    Name: string;
    Options: IOption[];
}

export interface IOption {
    ID: string;
    OptionType: OptionType;
    Label: string;
    DefaultValue?: string;
    IsRequired: boolean;
    SortOrder?: number;
}

export interface ILogin {
    ID: number;
    Username: string;
    Password: string;
    Domain?: string;
}

export interface IConnString {
    ID: number;
    Name: string;
    EnvironmentID: number;
    Value: string;
}

export interface IEmailSettings {
    From: string;
    Server: string;
    Port: number;
    Username: string;
    Password: string;
    TLS: boolean;
}
export interface IClickatellSettings {
    ApiKey: string;
    ApiUrl: string;
    From: string;
}

export interface IGlobalSettings {
    Email?: IEmailSettings;
    Clickatell?: IClickatellSettings;
    AuthenticationGroup?: string;
    SlackToken?: string;
    CleanupSchedule?: string;
    ResultAggregateDays?: number;
    ResultRetentionMonths?: number;
    Url?: string;
    LoginExpireAfterDays?: number;
    TimeZoneId?: string;
}

export interface ISettings {
    Logins: ILogin[];
    ConnStrings: IConnString[];
    Environments: IEnvironment[];
    Contacts: IContact[];
    CheckGroups: ICheckGroup[];
    Global: IGlobalSettings;
}

export interface IRunLog {
    Type: CheckLogType;
    Message: string;
}

export interface ISubCheckType {
    ID: number;
    Name: string;
    Options: IOption[];
}

export interface ISubCheck {
    ID: number;
    CheckID: number;
    TypeID: number;
    Active: number;
    Options: any;
}

export interface ICheckNotification {
    ID: number;
    TypeID: number;
    CheckID: number;
    Active: boolean;
    Options: any;
    FailCount?: number;
    FailMinutes?: number;
}

export interface ICheckNotificationType {
    ID: number;
    Name: string;
    Options: IOption[];
}

export interface ISlackChannel {
    ID: string;
    Name: string;
}

export interface IEnvironment {
    ID: number;
    Name: string;
}

export interface ICheckResults {
    MinDate: string;
    MaxDate: string;
    Results: ICheckResult[];
}

export interface IContactType {
    ID: string;
    Name: string;
}

export interface IContact {
    ID: string;
    TypeID: number;
    Name: string;
    Value: string;
}

export interface ICheckGroup {
    ID: string;
    Name: string;
}

export interface ILoginResult {
    Success: boolean;
    Token: string;
    Error: string;
    InitRequired: boolean;
}

export interface IApiKey {
    ID: number;
    UserID: number;
    Name: string;
    Key: string;
}

export interface IUser {
    ID: number;
    Username: string;
    Password?: string;
    IsWindowsUser: boolean;
    ApiKeys: IApiKey[];
}

export interface IInitRequest {
    Username: string;
    Password: string;
}

export interface IInitResult {
    Required: boolean;
}

export interface IValidateCronResult {
    Valid: boolean;
    Next?: string;
    Error?: string;
}
