using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemChecker.Model.Data;
using SystemChecker.Model.Enums;

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
