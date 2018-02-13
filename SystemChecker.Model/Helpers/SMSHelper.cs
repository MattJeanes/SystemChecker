using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SystemChecker.Model.Data;

namespace SystemChecker.Model.Helpers
{
    public interface ISMSHelper
    {
        Task SendSMS(SMSMessage message);
    }
    public class SMSMessage
    {
        public IEnumerable<string> To { get; set; }
        public string Message { get; set; }
    }
    public class SMSHelper : ISMSHelper
    {
        private class ClickatellRequest
        {
            [JsonProperty("text")]
            public string Text { get; set; }

            [JsonProperty("to")]
            public string[] To { get; set; }

            [JsonProperty("from")]
            public string From { get; set; }
        }
        private readonly ISettingsHelper _settingsHelper;
        private PhoneNumberUtil _phoneUtil;
        public SMSHelper(ISettingsHelper settingsHelper)
        {
            _settingsHelper = settingsHelper;
            _phoneUtil = PhoneNumberUtil.GetInstance();
        }

        public async Task SendSMS(SMSMessage message)
        {
            var settings = (await _settingsHelper.Get()).Global.Clickatell;
            if (settings == null || settings.ApiKey == null || settings.ApiUrl == null)
            {
                throw new Exception("Clickatell not set up");
            }
            var request = GetRequestFromMessage(message, settings);
            await SendMessage(request, settings);
        }

        private async Task SendMessage(ClickatellRequest request, ClickatellSettings settings)
        {
            var requestJson = JsonConvert.SerializeObject(request);
            var payload = new StringContent(requestJson, Encoding.UTF8, "application/json");
            payload.Headers.Add("X-Version", "1");

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", settings.ApiKey);

            var response = await httpClient.PostAsync(settings.ApiUrl, payload);
            var responseMessage = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(responseMessage);
            }
        }

        private ClickatellRequest GetRequestFromMessage(SMSMessage message, ClickatellSettings settings)
        {
            return new ClickatellRequest
            {
                To = message.To.Select(x => FormatNumber(x)).ToArray(),
                From = settings.From,
                Text = message.Message
            };
        }

        private string FormatNumber(string phone)
        {
            var number = _phoneUtil.Parse(phone, "GB");

            if (!_phoneUtil.IsValidNumber(number))
                throw new Exception($"Invalid phone number: {phone}");

            _phoneUtil.FormatOutOfCountryCallingNumber(number, "GB");

            return _phoneUtil.Format(number, PhoneNumberFormat.E164).Replace("+", "");
        }
    }
}
