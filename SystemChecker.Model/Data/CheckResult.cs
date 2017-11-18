using SystemChecker.Model.Enums;

namespace SystemChecker.Model.Data
{
    public interface ICheckResult
    {
        CheckResultStatus Status { get; set; }
    }
    public class CheckResult : ICheckResult
    {
        public CheckResultStatus Status { get; set; }
        public string Message { get; set; }
    }
}