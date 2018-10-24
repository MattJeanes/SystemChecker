using SystemChecker.Contracts.Enums;

namespace SystemChecker.Model.Loggers
{
    public abstract class BaseLogger : ICheckLogger
    {
        public void Info(string message)
        {
            Log(CheckLogType.Info, message);
        }

        public void Warn(string message)
        {
            Log(CheckLogType.Warn, message);
        }

        public void Error(string message)
        {
            Log(CheckLogType.Error, message);
        }

        public void Done(string message)
        {
            Log(CheckLogType.Done, message);
        }

        public abstract void Log(CheckLogType type, string message);
    }
}
