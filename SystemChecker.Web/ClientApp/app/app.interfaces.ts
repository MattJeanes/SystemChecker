import { CheckLogType, CheckResultStatus, OptionType } from "./app.enums";

export interface ICheck {
    ID: number;
    Name: string;
    Active: boolean;
    TypeID?: number;
    LastResultStatus?: CheckResultStatus;
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

export interface ICheckDetail extends ICheck {
    Schedules: ICheckSchedule[];
    Data: ICheckData;
    SubChecks: ISubCheck[];
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
    Value: string;
}

export interface ISettings {
    Logins: ILogin[];
    ConnStrings: IConnString[];
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
    Name: string;
    Active: number;
    Options: any;
}
