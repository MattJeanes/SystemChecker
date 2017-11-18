using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemChecker.Model.Enums;

namespace SystemChecker.Model.Loggers
{
    public class CheckLogger : BaseLogger
    {
        private readonly ILogger _logger;
        public CheckLogger(ILogger<CheckLogger> logger)
        {
            _logger = logger;
        }

        public override void Log(CheckLogType type, string message)
        {
            switch (type)
            {
                case CheckLogType.Info:
                    _logger.LogInformation(message);
                    break;
                case CheckLogType.Warn:
                    _logger.LogWarning(message);
                    break;
                case CheckLogType.Error:
                    _logger.LogError(message);
                    break;
                default:
                    _logger.LogDebug(message);
                    break;
            }
        }
    }
}
