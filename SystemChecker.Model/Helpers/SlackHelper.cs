using Microsoft.Extensions.Options;
using SlackAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SystemChecker.Model.Helpers
{
    public class SlackChannel
    {
        public string ID { get; set; }
        public string Name { get; set; }
    }
    public interface ISlackHelper
    {
        Task<IEnumerable<SlackChannel>> GetChannels();
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

        public async Task<IEnumerable<SlackChannel>> GetChannels()
        {
            var task = new TaskCompletionSource<IEnumerable<SlackChannel>>();
            var client = await GetClient();
            client.GetChannelList(response =>
            {
                if (response.ok)
                {
                    task.SetResult(response.channels.Select(x => new SlackChannel
                    {
                        ID = x.id,
                        Name = x.name
                    }));
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
