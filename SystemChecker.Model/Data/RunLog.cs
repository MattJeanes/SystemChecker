using SystemChecker.Model.Enums;

namespace SystemChecker.Model.Data
{
    public class RunLog
    {
        public CheckLogType Type { get; set; }
        public string Message { get; set; }
    }
}