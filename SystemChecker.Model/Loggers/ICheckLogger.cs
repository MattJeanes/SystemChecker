using SystemChecker.Contracts.Enums;

namespace SystemChecker.Model.Loggers
{
    public interface ICheckLogger
    {
        void Info(string message);
        void Warn(string message);
        void Error(string message);
        void Done(string message);
        void Log(CheckLogType type, string message);
    }
}
