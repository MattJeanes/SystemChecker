using System.Collections.Generic;
using System.Threading.Tasks;
using SystemChecker.Contracts.Data;
using SystemChecker.Contracts.DTO;

namespace SystemChecker.Contracts.Services
{
    public interface ISystemCheckerService
    {
        Task<List<CheckDTO>> GetAllAsync(bool? simpleStatus = null);
        Task<CheckDTO> GetByIdAsync(int id, bool? simpleStatus = null);
        Task<CheckDetailDTO> GetDetailsAsync(int id, bool? includeResults = null);
        Task<CheckResults> GetCheckResultsAsync(int id, string dateFrom, string dateTo);
        Task<List<CheckTypeDTO>> GetTypesAsync();
        Task<CheckerSettings> GetSettingsAsync();
        Task<CheckerSettings> SetSettingsAsync(CheckerSettings value);
        Task<CheckDetailDTO> EditAsync(CheckDetailDTO value);
        Task<List<RunLog>> RunAsync(int id);
        Task<bool> DeleteAsync(int id);
        Task<List<SubCheckTypeDTO>> GetSubCheckTypesAsync(int checkTypeID);
        Task<List<CheckNotificationTypeDTO>> GetCheckNotificationTypesAsync();
        Task<List<ChannelDTO>> GetSlackChannelsAsync();
        Task<List<ContactTypeDTO>> GetContactTypesAsync();
        Task<LoginResult> LoginAsync(LoginRequest request);
        Task<UserDTO> GetUserAsync();
        Task<UserDTO> EditUserAsync(UserDTO value);
        Task<InitResult> GetInitAsync();
        Task<InitResult> SetInitAsync(InitRequest request);
        Task<ValidateCronResult> ValidateCronExpressionAsync(string cron, bool? validateOnly);
    }
}