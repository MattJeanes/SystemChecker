using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemChecker.Model.Enums;

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
