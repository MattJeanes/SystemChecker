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
        TimeWarning = 2,
        // Everything above this is a warning
        Success = 1,
        Failed = 0,
        // Everything below this is a failure
        Timeout = -1,
    }
}
