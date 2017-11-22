export enum CheckTypeOptionType {
    Boolean = 1,
    String = 2,
    Number = 3,
    Date = 4,
    Login = 5,
    ConnString = 6,
}

export enum CheckLogType {
    Info = 1,
    Warn = 2,
    Error = 3,
    Done = 4,
}

export enum CheckResultStatus {
    TimeWarning = 2,
    // Everything above this is a warning
    Success = 1,
    Failed = 0,
    // Everything below this is a failure
    Timeout = -1,
}
