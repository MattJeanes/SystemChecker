using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SystemChecker.Model.Data;
using SystemChecker.Model.Data.Entities;
using SystemChecker.Model.Enums;
using SystemChecker.Model.Helpers;
using SystemChecker.Model.Loggers;

namespace SystemChecker.Model.Jobs
{
    public class WebRequestChecker : BaseChecker
    {
        public WebRequestChecker(ICheckerHelper helper) : base(helper) { }
        private enum Settings
        {
            RequestUrl = 5,
            Authentication = 6,
            TimeoutMS = 9,
            TimeWarnMS = 10,
        }

        public async override Task<CheckResult> PerformCheck(Check check, ISettings settings, ICheckLogger logger, CheckResult result)
        {
            ICredentials credentials = null;

            string url = check.Data.TypeOptions[((int)Settings.RequestUrl).ToString()];
            int? authId = check.Data.TypeOptions[((int)Settings.Authentication).ToString()];
            int timeoutMS = check.Data.TypeOptions[((int)Settings.TimeoutMS).ToString()];
            int? timeWarnMS = check.Data.TypeOptions[((int)Settings.TimeWarnMS).ToString()];

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

            var timer = new Stopwatch();
            using (var handler = new HttpClientHandler { Credentials = credentials })
            using (var client = new HttpClient(handler))
            {
                client.BaseAddress = new Uri(url);
                client.Timeout = TimeSpan.FromMilliseconds(timeoutMS);

                var headers = client.DefaultRequestHeaders;

                headers.UserAgent.TryParseAdd("SystemChecker");

                try
                {
                    timer.Start();
                    var response = await client.GetAsync("");
                    timer.Stop();
                    if (timer.ElapsedMilliseconds > timeWarnMS)
                    {
                        result.Status = CheckResultStatus.TimeWarning;
                    }
                    response.EnsureSuccessStatusCode(); // Throw in not success
                    logger.Info("Response headers:");
                    foreach (var header in response.Headers)
                    {
                        logger.Info($"\t{header.Key}: {String.Join(",", header.Value)}");
                    }
                    var responseText = await response.Content.ReadAsStringAsync();
                    logger.Info("Response text (truncated to 2048 characters):");
                    logger.Info(responseText.Substring(0, Math.Min(responseText.Length, 2048)));
                }
                catch (TaskCanceledException e) when (!e.CancellationToken.IsCancellationRequested)
                {
                    logger.Error($"Timeout after {timeoutMS}ms");
                    result.Status = CheckResultStatus.Timeout;
                }
                finally
                {
                    if (timer.IsRunning)
                    {
                        timer.Stop();
                    }
                    result.TimeMS = (int)timer.ElapsedMilliseconds;
                }
            }
            return result;
        }
    }
}
