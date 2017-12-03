using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public SlackNotifier(ISlackHelper slackHelper)
        {
            _slackHelper = slackHelper;
        }

        public override async Task SendNotification()
        {
            _logger.Info("Sending slack notification");
            string channelID = _notification.Options[((int)Options.ChannelId).ToString()];
            await _slackHelper.SendMessage(channelID, $"Check {_check.Name} completed");
        }
    }
}
