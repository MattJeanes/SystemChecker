using System.Collections.Generic;
using SystemChecker.Contracts.Data;
using SystemChecker.Contracts.Enums;

namespace SystemChecker.Model.Loggers
{
    public class ManualRunLogger : BaseLogger
    {
        private List<RunLog> _log = new List<RunLog>();

        public override void Log(CheckLogType type, string message)
        {
            _log.Add(new RunLog
            {
                Type = type,
                Message = message
            });
        }

        public List<RunLog> GetLog()
        {
            return _log;
        }
    }
}
