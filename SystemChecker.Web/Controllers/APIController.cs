using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SystemChecker.Contracts.Data;
using SystemChecker.Contracts.DTO;
using SystemChecker.Contracts.Services;
using SystemChecker.Model;
using SystemChecker.Model.Data.Entities;
using SystemChecker.Model.Data.Interfaces;
using SystemChecker.Model.Helpers;

namespace SystemChecker.Web.Controllers
{
    [Route("api")]
    public class APIController : Controller, ISystemCheckerService
    {
        private readonly ICheckRepository _checks;
        private readonly IRepository<LastResultStatus> _lastResultStatuses;
        private readonly IRepository<CheckResult> _checkResults;
        private readonly ISubCheckTypeRepository _subCheckTypes;
        private readonly ICheckTypeRepository _checkTypes;
        private readonly IRepository<ResultType> _resultTypes;
        private readonly IRepository<ResultStatus> _resultStatuses;
        private readonly ICheckNotificationTypeRepository _checkNotificationTypes;
        private readonly IRepository<ContactType> _contactTypes;
        private readonly IMapper _mapper;
        private readonly ISchedulerManager _manager;
        private readonly ISettingsHelper _settingsHelper;
        private readonly ISlackHelper _slackHelper;
        private readonly IJobHelper _jobHelper;
        private readonly ISecurityHelper _securityHelper;
        private readonly IUserRepository _users;
        private readonly ILogger _logger;
        public APIController(
            ICheckRepository checks,
            IRepository<LastResultStatus> lastResultStatuses,
            IRepository<CheckResult> checkResults,
            ISubCheckTypeRepository subCheckTypes,
            ICheckTypeRepository checkTypes,
            IRepository<ResultType> resultTypes,
            IRepository<ResultStatus> resultStatuses,
            ICheckNotificationTypeRepository checkNotificationTypes,
            IRepository<ContactType> contactTypes,
            IMapper mapper,
            ISchedulerManager manager,
            ISettingsHelper settingsHelper,
            ISlackHelper slackHelper,
            IJobHelper jobHelper,
            ISecurityHelper securityHelper,
            IUserRepository users,
            ILogger<APIController> logger
            )
        {
            _checks = checks;
            _lastResultStatuses = lastResultStatuses;
            _checkResults = checkResults;
            _subCheckTypes = subCheckTypes;
            _checkTypes = checkTypes;
            _resultTypes = resultTypes;
            _resultStatuses = resultStatuses;
            _checkNotificationTypes = checkNotificationTypes;
            _contactTypes = contactTypes;
            _mapper = mapper;
            _manager = manager;
            _settingsHelper = settingsHelper;
            _slackHelper = slackHelper;
            _jobHelper = jobHelper;
            _securityHelper = securityHelper;
            _users = users;
            _logger = logger;
        }

        [HttpGet]
        public async Task<List<CheckDTO>> GetAllAsync()
        {
            var checks = await _checks.GetAll().ToListAsync();
            var dtos = _mapper.Map<List<CheckDTO>>(checks);
            var lastResultStatuses = await _lastResultStatuses.GetAll()
                .Where(x => dtos.Select(y => y.ID).Contains(x.CheckID))
                .ToListAsync();
            foreach (var dto in dtos)
            {
                SetLastResultStatus(dto, lastResultStatuses.FirstOrDefault(x => x.CheckID == dto.ID));
            }
            return dtos;
        }

        [HttpGet("{id:int}")]
        public async Task<CheckDTO> GetByIdAsync(int id)
        {
            var check = await _checks.Find(id);
            var dto = _mapper.Map<CheckDTO>(check);
            var lastResultStatus = await _lastResultStatuses.FirstOrDefaultAsync(x => x.CheckID == check.ID);
            SetLastResultStatus(dto, lastResultStatus);
            return dto;
        }

        private void SetLastResultStatus(CheckDTO check, LastResultStatus lastResultStatus)
        {
            if (lastResultStatus == null)
            {
                check.LastResultStatus = null;
                check.LastResultType = null;
            }
            else
            {
                check.LastResultStatus = lastResultStatus.StatusID;
                check.LastResultType = lastResultStatus.TypeID;
            }
        }

        [HttpGet("details/{id:int}/{includeResults:bool?}")]
        public async Task<CheckDetailDTO> GetDetailsAsync(int id, bool? includeResults = null)
        {
            var check = await _checks.GetDetails(id, includeResults ?? false);
            return _mapper.Map<CheckDetailDTO>(check);
        }

        [HttpGet("results/{id:int}/{dateFrom}/{dateTo}")]
        public async Task<CheckResults> GetCheckResultsAsync(int id, string dateFrom, string dateTo)
        {
            var from = DateTimeOffset.Parse(dateFrom);
            from = from.Date + new TimeSpan(0, 0, 0);
            var to = DateTimeOffset.Parse(dateTo);
            to = to.Date + new TimeSpan(23, 59, 59);
            var min = await _checkResults.GetAll().Where(x => x.CheckID == id).OrderBy(x => x.DTS).FirstOrDefaultAsync();
            var max = await _checkResults.GetAll().Where(x => x.CheckID == id).OrderByDescending(x => x.DTS).FirstOrDefaultAsync();
            var results = await _checkResults.GetAll().Where(x => x.CheckID == id && x.DTS >= from && x.DTS < to).ToListAsync();
            return new CheckResults
            {
                MinDate = min?.DTS.LocalDateTime.ToString("yyyy-MM-dd"),
                MaxDate = max?.DTS.LocalDateTime.ToString("yyyy-MM-dd"),
                Results = _mapper.Map<List<CheckResultDTO>>(results),
            };
        }

        [HttpGet("types")]
        public async Task<List<CheckTypeDTO>> GetTypesAsync()
        {
            var types = await _checkTypes.GetAll().ToListAsync();
            return _mapper.Map<List<CheckTypeDTO>>(types);
        }

        [HttpGet("resulttypes")]
        public async Task<List<ResultTypeDTO>> GetResultTypesAsync()
        {
            var resultTypes = await _resultTypes.GetAll().ToListAsync();
            return _mapper.Map<List<ResultTypeDTO>>(resultTypes);
        }

        [HttpGet("resultstatuses")]
        public async Task<List<ResultStatusDTO>> GetResultStatusesAsync()
        {
            var resultStatuses = await _resultStatuses.GetAll().ToListAsync();
            return _mapper.Map<List<ResultStatusDTO>>(resultStatuses);
        }

        [HttpGet("settings")]
        public async Task<CheckerSettings> GetSettingsAsync()
        {
            return await _settingsHelper.Get();
        }

        [HttpPost("settings")]
        public async Task<CheckerSettings> SetSettingsAsync([FromBody]CheckerSettings value)
        {
            var global = await _settingsHelper.GetGlobal();
            await _settingsHelper.Set(value);
            if (global.TimeZoneId != value.Global.TimeZoneId)
            {
                _logger.LogInformation($"TimeZoneId changed from {global.TimeZoneId} to {value.Global.TimeZoneId}, updating schedules");
                await _manager.UpdateSchedules();
            }
            await _manager.UpdateMaintenanceJobs(value.Global);
            return await GetSettingsAsync();
        }

        [HttpPost]
        public async Task<CheckDetailDTO> EditAsync([FromBody]CheckDetailDTO value)
        {
            Check check;
            if (value.ID > 0)
            {
                check = await _checks.GetDetails(value.ID);
            }
            else
            {
                check = new Check
                {
                    Schedules = new List<CheckSchedule>()
                };
                _checks.Add(check);
            }

            _mapper.Map(value, check);

            await _checks.SaveChangesAsync();
            await _manager.UpdateSchedule(check);
            return await GetDetailsAsync(check.ID);
        }

        [HttpPost("run/{id:int}")]
        public async Task<List<RunLog>> RunAsync(int id)
        {
            var check = await _checks.GetDetails(id);
            var logger = await _jobHelper.RunManualUICheck(check);
            try
            {
                await _checks.SaveChangesAsync();
            }
            catch (Exception e)
            {
                logger.Error("Failed to commit changes");
                logger.Error(e.ToString());
            }
            return logger.GetLog();
        }

        [HttpDelete("{id:int}")]
        public async Task<bool> DeleteAsync(int id)
        {
            var check = await _checks.GetDetails(id, true);
            _checks.Delete(check);
            await _manager.RemoveSchedule(check);
            await _checks.SaveChangesAsync();
            return true;
        }

        [HttpGet("subchecktypes/{checkTypeID:int}")]
        public async Task<List<SubCheckTypeDTO>> GetSubCheckTypesAsync(int checkTypeId)
        {
            var subCheckTypes = await _subCheckTypes.GetAll().Where(x => x.CheckTypeID == checkTypeId).ToListAsync();
            return _mapper.Map<List<SubCheckTypeDTO>>(subCheckTypes);
        }

        [HttpGet("checknotificationtypes")]
        public async Task<List<CheckNotificationTypeDTO>> GetCheckNotificationTypesAsync()
        {
            var checkNotificationTypes = await _checkNotificationTypes.GetAll().ToListAsync();
            return _mapper.Map<List<CheckNotificationTypeDTO>>(checkNotificationTypes);
        }

        [HttpGet("slackchannels")]
        public async Task<List<SlackChannelDTO>> GetSlackChannelsAsync()
        {
            var channels = await _slackHelper.GetChannels();
            return _mapper.Map<List<SlackChannelDTO>>(channels);
        }

        [HttpGet("contacttypes")]
        public async Task<List<ContactTypeDTO>> GetContactTypesAsync()
        {
            var contactTypes = await _contactTypes.GetAll().ToListAsync();
            return _mapper.Map<List<ContactTypeDTO>>(contactTypes);
        }

        [HttpPost("login")]
        public async Task<LoginResult> LoginAsync([FromBody] LoginRequest request)
        {
            return await _securityHelper.Login(request, HttpContext);
        }

        [HttpGet("user")]
        public async Task<UserDTO> GetUserAsync()
        {
            var username = (string)HttpContext.Items["Username"];
            var user = await _users.GetDetails(username);
            if (user == null)
            {
                throw new Exception($"User '{username}' not found");
            }
            return _mapper.Map<UserDTO>(user);
        }

        [HttpPost("user")]
        public async Task<UserDTO> EditUserAsync([FromBody]UserDTO value)
        {
            var username = (string)HttpContext.Items["Username"];
            if (value.Username != username)
            {
                throw new Exception("Cannot edit other users");
            }

            Model.Data.Entities.User user;
            if (value.ID > 0)
            {
                user = await _users.GetDetails(value.ID);
            }
            else
            {
                user = new Model.Data.Entities.User
                {
                    ApiKeys = new List<ApiKey>()
                };
                _users.Add(user);
            }

            if (!string.IsNullOrEmpty(value.Password))
            {
                value.Password = SecurePasswordHasher.Hash(value.Password);
            }

            _mapper.Map(value, user);

            await _users.SaveChangesAsync();
            return await GetUserAsync();
        }

        [HttpGet("init")]
        public async Task<InitResult> GetInitAsync()
        {
            var result = new InitResult();

            var anyUsers = await _users.GetAll().AnyAsync(x => !x.IsWindowsUser);
            result.Required = !anyUsers;

            return result;
        }

        [HttpPost("init")]
        public async Task<InitResult> SetInitAsync([FromBody]InitRequest request)
        {
            var result = new InitResult();

            if (string.IsNullOrWhiteSpace(request.Username))
            {
                throw new Exception("Username required");
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                throw new Exception("Password required");
            }

            var user = await _securityHelper.CreateUser(request.Username, request.Password);
            result.Required = false;

            return result;
        }

        [HttpPost("cron/{validateOnly:bool?}")]
        public async Task<ValidateCronResult> ValidateCronExpressionAsync([FromBody]string cron, bool? validateOnly = null)
        {
            try
            {
                var expression = new CronExpression(cron)
                {
                    TimeZone = await _jobHelper.GetTimeZone()
                };
                if (validateOnly ?? false)
                {
                    return new ValidateCronResult
                    {
                        Valid = true
                    };
                }
                DateTimeOffset? time = DateTimeOffset.UtcNow;
                var count = 5;
                var next = "";
                while (count > 0)
                {
                    count--;
                    time = expression.GetTimeAfter(time.Value);
                    if (!time.HasValue)
                    {
                        break;
                    }
                    next += time.Value.LocalDateTime.ToString("yyyy-mm-dd HH:mm:ss") + "\n";
                }
                next += "\n" + expression.GetExpressionSummary();
                return new ValidateCronResult
                {
                    Valid = true,
                    Next = next
                };
            }
            catch (Exception e)
            {
                return new ValidateCronResult
                {
                    Valid = false,
                    Error = e.Message
                };
            }
        }
    }
}
