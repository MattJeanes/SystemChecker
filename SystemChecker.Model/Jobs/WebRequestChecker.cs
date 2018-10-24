using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SystemChecker.Contracts.Enums;
using SystemChecker.Model.Data.Entities;
using SystemChecker.Model.Helpers;

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
            HttpMethod = 14,
        }
        private enum SubChecks
        {
            ResponseContains = 1,
            JSONProperty = 2,
        }
        private enum ResponseContains
        {
            Text = 1,
        }
        private enum JSONProperty
        {
            FieldName = 2,
            Exists = 3,
            ValueContains = 4,
        }

        public async override Task<CheckResult> PerformCheck(CheckResult result)
        {
            ICredentials credentials = null;

            string url = _check.Data.TypeOptions[((int)Settings.RequestUrl).ToString()];
            int? authId = _check.Data.TypeOptions[((int)Settings.Authentication).ToString()];
            int timeoutMS = _check.Data.TypeOptions[((int)Settings.TimeoutMS).ToString()];
            int? timeWarnMS = _check.Data.TypeOptions[((int)Settings.TimeWarnMS).ToString()];
            string httpMethod = _check.Data.TypeOptions[((int)Settings.HttpMethod).ToString()];

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
                        result.Status = CheckResultStatus.TimeWarning;
                    }
                    _logger.Info("Request headers:");
                    foreach (var header in response.RequestMessage.Headers)
                    {
                        _logger.Info($"\t{header.Key}: {String.Join(",", header.Value)}");
                    }
                    _logger.Info("Response headers:");
                    foreach (var header in response.Headers)
                    {
                        _logger.Info($"\t{header.Key}: {String.Join(",", header.Value)}");
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

        private void RunSubCheck(SubCheck subCheck, string responseText, CheckResult result)
        {
            switch (subCheck.TypeID)
            {
                case (int)SubChecks.ResponseContains:
                    string text = subCheck.Options[((int)ResponseContains.Text).ToString()];
                    if (responseText.Contains(text))
                    {
                        _logger.Info($"Response contains '{text}'");
                    }
                    else
                    {
                        throw new SubCheckException($"Response does not contain '{text}'");
                    }
                    break;
                case (int)SubChecks.JSONProperty:
                    string fieldName = subCheck.Options[((int)JSONProperty.FieldName).ToString()];
                    bool exists = subCheck.Options[((int)JSONProperty.Exists).ToString()];
                    string valueContains = subCheck.Options[((int)JSONProperty.ValueContains).ToString()];
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
