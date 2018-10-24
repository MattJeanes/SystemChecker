using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SystemChecker.Model.Data.Entities;
using SystemChecker.Model.Data.Interfaces;
using SystemChecker.Model.Helpers;

namespace SystemChecker.Model.Notifiers
{
    public class SMSNotifier : BaseNotifier
    {
        private enum Options
        {
            PhoneNumbers = 3
        }
        private readonly ISMSHelper _helper;
        public SMSNotifier(IRepository<CheckResult> checkResult, ISMSHelper helper) : base(checkResult)
        {
            _helper = helper;
        }

        protected override async Task SendNotification(NotificationType type, string message, string failedAfter = null)
        {
            List<int> phoneIDs = _notification.Options[((int)Options.PhoneNumbers).ToString()].ToObject<List<int>>();
            var phoneNumbers = _settings.Contacts.Where(x => phoneIDs.Contains(x.ID)).Select(x => x.Value);
            var to = string.Join(",", phoneNumbers.ToArray());
            _logger.Info($"Sending SMS notification to {to}: {message}");

            await _helper.SendSMS(new SMSMessage
            {
                To = phoneNumbers,
                Message = $"SystemChecker: {message}"
            });
        }
    }
}
