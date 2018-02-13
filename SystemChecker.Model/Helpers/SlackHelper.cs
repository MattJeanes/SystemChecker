using Microsoft.Extensions.Options;
using SlackAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemChecker.Model.Helpers
{
    public interface ISlackHelper
    {
        Task<Channel[]> GetChannels();
        Task SendMessage(string channelID, string message);
    }
    public class SlackHelper : ISlackHelper
    {
        private readonly AppSettings _appSettings;
        private readonly ISettingsHelper _settingsHelper;
        private SlackClient _client;
        public SlackHelper(IOptions<AppSettings> appSettings, ISettingsHelper settingsHelper)
        {
            _appSettings = appSettings.Value;
            _settingsHelper = settingsHelper;
        }

        private async Task<SlackClient> GetClient()
        {
            if (_client != null) { return _client; }
            var settings = await _settingsHelper.Get();
            var token = settings.Global.SlackToken;
            if (string.IsNullOrEmpty(token))
            {
                throw new Exception("Slack token not setup");
            }
            _client = new SlackClient(token);
            return _client;
        }

        public async Task<Channel[]> GetChannels()
        {
            var task = new TaskCompletionSource<Channel[]>();
            var client = await GetClient();
            client.GetChannelList(response =>
            {
                if (response.ok)
                {
                    task.SetResult(response.channels);
                }
                else
                {
                    task.SetException(new Exception(response.error));
                }
            });
            return await task.Task;
        }

        public async Task SendMessage(string channelID, string message)
        {
            var task = new TaskCompletionSource<bool>();
            var client = await GetClient();
            client.PostMessage(response =>
            {
                if (response.ok)
                {
                    task.SetResult(true);
                }
                else
                {
                    task.SetException(new Exception(response.error));
                }
            }, channelID, message);
            await task.Task;
        }
    }
}
