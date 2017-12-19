using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemChecker.Model.Data.Interfaces;

namespace SystemChecker.Model.Notifiers
{
    public class EmailNotifier : BaseNotifier
    {
        private enum Options
        {
            EmailAddresses = 2
        }
        public EmailNotifier(ICheckerUow uow) : base(uow) { }

        protected override async Task SendNotification(NotificationType type, string message, string failedAfter = null)
        {
            List<int> emailIDs = _notification.Options[((int)Options.EmailAddresses).ToString()].ToObject<List<int>>();
            var emailAddresses = _settings.Contacts.Where(x => emailIDs.Contains(x.ID)).Select(x => x.Value);
            var to = string.Join(",", emailAddresses.ToArray());
            _logger.Info($"Sending email notification to {to}: {message}");
        }
    }
}
