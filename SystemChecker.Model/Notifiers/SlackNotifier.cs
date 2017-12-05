using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemChecker.Model.Data.Interfaces;
using SystemChecker.Model.Helpers;

namespace SystemChecker.Model.Notifiers
{
    public class SlackNotifier : BaseNotifier
    {
        private enum Options
        {
            ChannelId = 1
        }
        private readonly ISlackHelper _slackHelper;
        public SlackNotifier(ICheckerUow uow, ISlackHelper slackHelper) : base(uow)
        {
            _slackHelper = slackHelper;
        }

        protected override async Task SendNotification(NotificationType type, string failedAfter = null)
        {
            string channelID = _notification.Options[((int)Options.ChannelId).ToString()];
            var message = $"Check {_check.Name} completed in {_result.TimeMS}ms with result: {_result.Status.ToString()}{(string.IsNullOrEmpty(failedAfter) ? "" :  $" - failed after {failedAfter}")}";
            _logger.Info($"Sending slack notification to channel ID {channelID}: {message}");
            await _slackHelper.SendMessage(channelID, message);
        }
    }
}
