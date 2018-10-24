using SystemChecker.Contracts.Enums;

namespace SystemChecker.Contracts.Data
{
    public class RunLog
    {
        public CheckLogType Type { get; set; }
        public string Message { get; set; }
    }
}