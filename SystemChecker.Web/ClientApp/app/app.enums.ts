export enum RequestType {
    GetAll,
    GetDetails,
    Edit,
    Delete,
    GetTypes,
    GetSettings,
    SetSettings,
    Run,
}

export enum ResponseType {
    Run,
}

export enum CheckTypeOptionType {
    Boolean = 1,
    String = 2,
    Number = 3,
    Date = 4,
    Login = 5,
    ConnString = 6,
}

export enum CheckLogType {
    Info,
    Warn,
    Error,
    Done,
}
