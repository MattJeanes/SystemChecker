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
        protected ICheckLogger _logger;
        public async Task Run(Check check, CheckNotification notification, ICheckLogger logger)
        {
            _check = check;
            _notification = notification;
            _logger = logger;
            await SendNotification();
        }

        public abstract Task SendNotification();
    }
}
