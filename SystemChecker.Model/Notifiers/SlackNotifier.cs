using Microsoft.Extensions.Options;
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
        private readonly AppSettings _appSettings;
        public SlackNotifier(ICheckerUow uow, ISlackHelper slackHelper, IOptions<AppSettings> appSettings) : base(uow)
        {
            _slackHelper = slackHelper;
            _appSettings = appSettings.Value;
        }

        protected override async Task SendNotification(NotificationType type, string message, string failedAfter = null)
        {
            var url = $"{_appSettings.Url}/details/{_check.ID}";
            message = $"Check <{url}|{_check.Name}> completed in {_result.TimeMS}ms with result: {_result.Status.ToString()}";
            string channelID = _notification.Options[((int)Options.ChannelId).ToString()];
            _logger.Info($"Sending slack notification to channel ID {channelID}: {message}");
            await _slackHelper.SendMessage(channelID, message);
        }
    }
}
