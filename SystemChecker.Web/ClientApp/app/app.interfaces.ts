import { CheckLogType, CheckTypeOptionType } from "./app.enums";

export interface ICheck {
    ID: number;
    Name: string;
    Active: boolean;
    TypeID?: number;
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
}

export interface ICheckType {
    ID: number;
    Name: string;
    Options: ICheckTypeOption[];
}

export interface ICheckTypeOption {
    ID: number;
    OptionType: CheckTypeOptionType;
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
    ConnString: string;
}

export interface ISettings {
    Logins: ILogin[];
    ConnStrings: IConnString[];
}

export interface IRunLog {
    Type: CheckLogType;
    Message: string;
}
