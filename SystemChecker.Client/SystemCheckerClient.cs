using Moneybarn.Common.Http;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SystemChecker.Contracts.Data;
using SystemChecker.Contracts.DTO;

namespace SystemChecker.Client
{
    public class SystemCheckerClient : ISystemCheckerClient
    {
        private readonly IWebServiceHelper _client;
        public SystemCheckerClient(string url, string apiKey)
        {
            _client = new WebServiceHelper(url).AddOptions(new WebServiceHelperOptions
            {
                Headers = new Dictionary<string, string>
                {
                    ["ApiKey"] = apiKey
                }
            });
        }

        public async Task<List<CheckDTO>> GetAllAsync()
        {
            return await _client.CallAsync<List<CheckDTO>>(HttpMethod.Get, string.Empty);
        }

        public async Task<CheckDTO> GetByIdAsync(int id)
        {
            return await _client.CallAsync<CheckDTO>(HttpMethod.Get, id.ToString());
        }

        public async Task<CheckDetailDTO> GetDetailsAsync(int id, bool? includeResults = null)
        {
            return await _client.CallAsync<CheckDetailDTO>(HttpMethod.Get, $"details/{id}/{(includeResults.HasValue ? includeResults.Value.ToString() : string.Empty)}");
        }

        public async Task<CheckResults> GetCheckResultsAsync(int id, string dateFrom, string dateTo)
        {
            return await _client.CallAsync<CheckResults>(HttpMethod.Get, $"results/{id}/{dateFrom}/{dateTo}");
        }

        public async Task<List<CheckTypeDTO>> GetTypesAsync()
        {
            return await _client.CallAsync<List<CheckTypeDTO>>(HttpMethod.Get, "types");
        }

        public async Task<List<ResultTypeDTO>> GetResultTypesAsync()
        {
            return await _client.CallAsync<List<ResultTypeDTO>>(HttpMethod.Get, "resulttypes");
        }

        public async Task<List<ResultStatusDTO>> GetResultStatusesAsync()
        {
            return await _client.CallAsync<List<ResultStatusDTO>>(HttpMethod.Get, "resultstatuses");
        }

        public async Task<CheckerSettings> GetSettingsAsync()
        {
            return await _client.CallAsync<CheckerSettings>(HttpMethod.Get, "settings");
        }

        public async Task<CheckerSettings> SetSettingsAsync(CheckerSettings value)
        {
            return await _client.CallAsync<CheckerSettings>(HttpMethod.Post, "settings", value);
        }

        public async Task<CheckDetailDTO> EditAsync(CheckDetailDTO value)
        {
            return await _client.CallAsync<CheckDetailDTO>(HttpMethod.Post, string.Empty, value);
        }

        public async Task<List<RunLog>> RunAsync(int id)
        {
            return await _client.CallAsync<List<RunLog>>(HttpMethod.Post, $"run/{id}");
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _client.CallAsync<bool>(HttpMethod.Delete, id.ToString());
        }

        public async Task<List<SubCheckTypeDTO>> GetSubCheckTypesAsync(int checkTypeId)
        {
            return await _client.CallAsync<List<SubCheckTypeDTO>>(HttpMethod.Get, $"subchecktypes/{checkTypeId}");
        }

        public async Task<List<CheckNotificationTypeDTO>> GetCheckNotificationTypesAsync()
        {
            return await _client.CallAsync<List<CheckNotificationTypeDTO>>(HttpMethod.Get, "checknotificationtypes");
        }

        public async Task<List<SlackChannelDTO>> GetSlackChannelsAsync()
        {
            return await _client.CallAsync<List<SlackChannelDTO>>(HttpMethod.Get, "slackchannels");
        }

        public async Task<List<ContactTypeDTO>> GetContactTypesAsync()
        {
            return await _client.CallAsync<List<ContactTypeDTO>>(HttpMethod.Get, "contacttypes");
        }

        public async Task<LoginResult> LoginAsync(LoginRequest request)
        {
            return await _client.CallAsync<LoginResult>(HttpMethod.Post, "login", request);
        }

        public async Task<UserDTO> GetUserAsync()
        {
            return await _client.CallAsync<UserDTO>(HttpMethod.Get, "user");
        }

        public async Task<UserDTO> EditUserAsync(UserDTO value)
        {
            return await _client.CallAsync<UserDTO>(HttpMethod.Post, "user", value);
        }

        public async Task<InitResult> GetInitAsync()
        {
            return await _client.CallAsync<InitResult>(HttpMethod.Get, "init");
        }

        public async Task<InitResult> SetInitAsync(InitRequest request)
        {
            return await _client.CallAsync<InitResult>(HttpMethod.Post, "init", request);
        }

        public async Task<ValidateCronResult> ValidateCronExpressionAsync(string cron, bool? validateOnly = null)
        {
            return await _client.CallAsync<ValidateCronResult>(HttpMethod.Get, $"cron/{(validateOnly.HasValue ? validateOnly.Value.ToString() : string.Empty)}", cron);
        }
    }
}
