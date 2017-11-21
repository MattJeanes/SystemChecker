using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SystemChecker.Model.Data;
using SystemChecker.Model.Data.Entities;
using SystemChecker.Model.Enums;
using SystemChecker.Model.Loggers;

namespace SystemChecker.Model.Jobs
{
    public class WebRequestChecker : BaseChecker
    {
        public WebRequestChecker(ISchedulerManager manager) : base(manager) { }
        private enum Settings
        {
            RequestUrl = 5,
            Authentication = 6,
        }

        public async override Task<ICheckResult> PerformCheck(Check check, ISettings settings, ICheckLogger logger)
        {
            ICredentials credentials = null;

            var result = new CheckResult { Status = CheckResultStatus.Success };
            string url = check.Data.TypeOptions[((int)Settings.RequestUrl).ToString()];
            int? authId = check.Data.TypeOptions[((int)Settings.Authentication).ToString()];
            if (authId.HasValue && authId.Value > 0)
            {
                var login = settings.Logins.FirstOrDefault(x => x.ID == authId);
                if (login == null)
                {
                    throw new Exception("Login not found");
                }
                logger.Info($"Using login {login.Username}");
                credentials = new NetworkCredential(login.Username, login.Password, login.Domain);
            }

            using (var handler = new HttpClientHandler { Credentials = credentials })
            using (var client = new HttpClient(handler))
            {
                try
                {
                    client.BaseAddress = new Uri(url);
                    client.Timeout = TimeSpan.FromSeconds(5);

                    var headers = client.DefaultRequestHeaders;

                    headers.UserAgent.TryParseAdd("SystemChecker");

                    var response = await client.GetAsync("");
                    response.EnsureSuccessStatusCode(); // Throw in not success

                    var responseText = await response.Content.ReadAsStringAsync();
                    logger.Info("Response text:");
                    logger.Info(responseText);
                }
                catch (HttpRequestException wex)
                {
                    logger.Error($"WebException : {wex}");

                    return new CheckResult
                    {
                        Status = CheckResultStatus.Failed,
                        Message = wex.Message
                    };
                }
            }
            return result;
        }
    }
}
