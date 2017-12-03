using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemChecker.Model.Data.Entities;
using SystemChecker.Model.Loggers;

namespace SystemChecker.Model.Notifiers
{
    public abstract class BaseNotifier
    {
        protected Check _check;
        protected CheckNotification _notification;
        protected CheckResult _result;
        protected ICheckLogger _logger;
        public async Task Run(Check check, CheckNotification notification, CheckResult result, ICheckLogger logger)
        {
            _check = check;
            _notification = notification;
            _result = result;
            _logger = logger;
            await SendNotification();
        }

        public abstract Task SendNotification();
    }
}
