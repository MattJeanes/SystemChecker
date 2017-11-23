using System;
using System.Collections.Generic;
using System.Text;

namespace SystemChecker.Model.Enums
{
    /// <summary>
    /// Greater than 1 is Success with warnings
    /// Less than 0 is Specific Failure e.g. Timeout
    /// </summary>
    public enum CheckResultStatus
    {
        TimeWarning = 3,
        Warning = 2,
        // Everything above this is a warning
        Success = 1,
        NotRun = 0,
        Failed = -1,
        // Everything below this is a failure
        Timeout = -2,
    }
}
