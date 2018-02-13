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
    ID: number;
    OptionType: OptionType;
    Label: string;
    DefaultValue?: string;
    IsRequired: boolean;
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
    name: string;
    creator: string;
    is_archived: boolean;
    is_member: boolean;
    is_general: boolean;
    is_channel: boolean;
    is_group: boolean;
    num_members: number;
    members: string[];
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
