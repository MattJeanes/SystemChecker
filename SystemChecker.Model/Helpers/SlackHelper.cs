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
        private readonly SlackClient _client;
        public SlackHelper(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
            _client = new SlackClient(_appSettings.SlackToken);
        }

        public async Task<Channel[]> GetChannels()
        {
            var task = new TaskCompletionSource<Channel[]>();
            _client.GetChannelList(response => {
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
            _client.PostMessage(response =>
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
