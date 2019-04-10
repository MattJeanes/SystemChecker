using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SystemChecker.Model.Data.Entities;
using SystemChecker.Model.Data.Enums;
using SystemChecker.Model.Helpers;

namespace SystemChecker.Model.Jobs
{
    public class WebRequestChecker : BaseChecker
    {
        public WebRequestChecker(ICheckerHelper helper) : base(helper) { }

        public class Settings
        {
            public string RequestUrl { get; set; }
            public int? Authentication { get; set; }
            public int TimeoutMS { get; set; }
            public int? TimeWarnMS { get; set; }
            public string HttpMethod { get; set; }
        }

        public enum SubChecks
        {
            ResponseContains,
            JsonProperty
        }

        public class ResponseContains
        {
            public string Text { get; set; }
        }

        public class JsonProperty
        {
            public string FieldName { get; set; }
            public bool Exists { get; set; }
            public string Value { get; set; }
        }

        public override async Task<CheckResult> PerformCheck(CheckResult result)
        {
            ICredentials credentials = null;

            var settings = _check.Data.GetTypeOptions<Settings>();

            var url = settings.RequestUrl;
            var authId = settings.Authentication;
            var timeoutMS = settings.TimeoutMS;
            var timeWarnMS = settings.TimeWarnMS;
            var httpMethod = settings.HttpMethod;

            if (authId.HasValue && authId.Value > 0)
            {
                var login = _settings.Logins.FirstOrDefault(x => x.ID == authId);
                if (login == null)
                {
                    throw new Exception("Login not found");
                }
                _logger.Info($"Using login {login.Username}");
                credentials = new NetworkCredential(login.Username, login.Password, login.Domain);
            }

            var timer = new Stopwatch();
            // If this fails with a 401.1 IIS error when calling a site on the same server, see: https://serverfault.com/questions/485006/why-cant-i-log-in-to-a-windows-protected-iis-7-5-directory-on-the-server
            using (var handler = new HttpClientHandler { Credentials = credentials })
            using (var client = new HttpClient(handler))
            {
                var request = new HttpRequestMessage(new HttpMethod(httpMethod ?? "GET"), url);
                request.Headers.UserAgent.TryParseAdd("SystemChecker");

                try
                {
                    _logger.Info($"Calling {url}");
                    HttpResponseMessage response;
                    using (var cts = new CancellationTokenSource(timeoutMS))
                    {
                        timer.Start();
                        response = await client.SendAsync(request, cts.Token);
                        timer.Stop();
                    }

                    if (timer.ElapsedMilliseconds > timeWarnMS)
                    {
                        result.Status = _helper.GetResultStatus(ResultStatusEnum.TimeWarning);
                    }
                    _logger.Info("Request headers:");
                    foreach (var header in response.RequestMessage.Headers)
                    {
                        _logger.Info($"\t{header.Key}: {string.Join(",", header.Value)}");
                    }
                    _logger.Info("Response headers:");
                    foreach (var header in response.Headers)
                    {
                        _logger.Info($"\t{header.Key}: {string.Join(",", header.Value)}");
                    }
                    var responseText = await response.Content.ReadAsStringAsync();
                    _logger.Info("Response text (truncated to 2048 characters):");
                    _logger.Info(responseText.Substring(0, Math.Min(responseText.Length, 2048)));
                    response.EnsureSuccessStatusCode(); // Throw in not success
                    await _helper.RunSubChecks(_check, _logger, subCheck => RunSubCheck(subCheck, responseText, result));
                }
                catch (TaskCanceledException e) when (!e.CancellationToken.IsCancellationRequested)
                {
                    _logger.Error($"Timeout after {timeoutMS}ms");
                    result.Status = _helper.GetResultStatus(ResultStatusEnum.Timeout);
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

        private void RunSubCheck(SubCheck subCheck, string responseText, CheckResult result)
        {
            switch (subCheck.Type.Identifier)
            {
                case nameof(SubChecks.ResponseContains):
                    var responseContainsOptions = subCheck.GetOptions<ResponseContains>();
                    var text = responseContainsOptions.Text;
                    if (responseText.Contains(text))
                    {
                        _logger.Info($"Response contains '{text}'");
                    }
                    else
                    {
                        throw new SubCheckException($"Response does not contain '{text}'");
                    }
                    break;
                case nameof(SubChecks.JsonProperty):
                    var jsonPropertyOptions = subCheck.GetOptions<JsonProperty>();
                    var fieldName = jsonPropertyOptions.FieldName;
                    var exists = jsonPropertyOptions.Exists;
                    var valueContains = jsonPropertyOptions.Value;
                    var obj = JObject.Parse(responseText);
                    var value = obj.SelectToken(fieldName)?.ToString();
                    if (value == null && exists)
                    {
                        throw new SubCheckException($"Field '{fieldName}' does not exist");
                    }
                    else if (value != null && !exists)
                    {
                        throw new SubCheckException($"Field '{fieldName}' exists");
                    }
                    if (value != null)
                    {
                        if (!string.IsNullOrWhiteSpace(valueContains))
                        {
                            if (value.Contains(valueContains))
                            {

                                _logger.Info($"Field '{fieldName}' contains '{valueContains}'");
                            }
                            else
                            {
                                throw new SubCheckException($"Field '{fieldName}' does not contain '{valueContains}'");
                            }
                        }
                    }
                    break;
                default:
                    _logger.Warn($"Unknown sub-check type {subCheck.TypeID} - ignoring");
                    break;
            }
        }
    }
}
