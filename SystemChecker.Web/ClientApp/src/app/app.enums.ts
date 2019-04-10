export enum OptionType {
    Boolean = 1,
    String = 2,
    Number = 3,
    Date = 4,
    Login = 5,
    ConnString = 6,
    Text = 7,
    Slack = 8,
    Environment = 9,
    Email = 10,
    Phone = 11,
    HttpMethod = 12,
}

export enum CheckLogType {
    Info = 1,
    Warn = 2,
    Error = 3,
    Done = 4,
}

export enum ContactType {
    Email = 1,
    Phone = 2,
}

export enum ResultType {
    Success,
    Failed,
    Warning,
    NotRun,
}
