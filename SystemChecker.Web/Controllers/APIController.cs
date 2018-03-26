using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SystemChecker.Model.Data;
using SystemChecker.Model.Data.Entities;
using SystemChecker.Model.Enums;
using SystemChecker.Model.Data.Interfaces;
using SystemChecker.Model.DTO;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Quartz;
using SystemChecker.Model;
using SystemChecker.Model.Helpers;
using SlackAPI;
using Microsoft.Extensions.Logging;

namespace SystemChecker.Web.Controllers
{
    [Route("api")]
    public class APIController : Controller
    {
        private readonly ICheckRepository _checks;
        private readonly IRepository<CheckResult> _checkResults;
        private readonly ISubCheckTypeRepository _subCheckTypes;
        private readonly ICheckTypeRepository _checkTypes;
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
        public APIController(ICheckRepository checks, IRepository<CheckResult> checkResults, ISubCheckTypeRepository subCheckTypes, ICheckTypeRepository checkTypes, ICheckNotificationTypeRepository checkNotificationTypes,
            IRepository<ContactType> contactTypes, IMapper mapper, ISchedulerManager manager, ISettingsHelper settingsHelper, ISlackHelper slackHelper, IJobHelper jobHelper, ISecurityHelper securityHelper,
            IUserRepository users, ILogger<APIController> logger)
        {
            _checks = checks;
            _checkResults = checkResults;
            _subCheckTypes = subCheckTypes;
            _checkTypes = checkTypes;
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

        [HttpGet("{simpleStatus:bool?}")]
        public async Task<List<CheckDTO>> GetAll(bool? simpleStatus)
        {
            var checks = await _checks.GetAll().ToListAsync();
            var dtos = _mapper.Map<List<CheckDTO>>(checks);
            foreach (var check in dtos)
            {
                await SetLastResultStatus(check, simpleStatus ?? false);
            }
            return dtos;
        }

        [HttpGet("{id:int}/{simpleStatus:bool?}")]
        public async Task<CheckDTO> GetByID(int id, bool? simpleStatus)
        {
            var check = await _checks.Find(id);
            var dto = _mapper.Map<CheckDTO>(check);
            await SetLastResultStatus(dto, simpleStatus ?? false);
            return dto;
        }

        private async Task SetLastResultStatus(CheckDTO check, bool simpleStatus = true)
        {
            var result = await _checkResults.GetAll().Where(x => x.CheckID == check.ID).OrderByDescending(x => x.ID).FirstOrDefaultAsync();
            var status = result?.Status;
            if (status == null)
            {
                check.LastResultStatus = CheckResultStatus.NotRun;
            }
            else if (!simpleStatus)
            {
                check.LastResultStatus = status;
            }
            else if (status > CheckResultStatus.Success)
            {
                check.LastResultStatus = CheckResultStatus.Warning;
            }
            else if (status == CheckResultStatus.Success)
            {
                check.LastResultStatus = CheckResultStatus.Success;
            }
            else
            {
                check.LastResultStatus = CheckResultStatus.Failed;
            }
        }

        [HttpGet("details/{id:int}/{includeResults:bool?}")]
        public async Task<CheckDetailDTO> GetDetails(int id, bool? includeResults = null)
        {
            var check = await _checks.GetDetails(id, includeResults ?? false);
            return _mapper.Map<CheckDetailDTO>(check);
        }

        [HttpGet("results/{id:int}/{dateFrom}/{dateTo}")]
        public async Task<ICheckResults> GetCheckResults(int id, string dateFrom, string dateTo)
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
        public async Task<List<CheckTypeDTO>> GetTypes()
        {
            var types = await _checkTypes.GetAll().ToListAsync();
            return _mapper.Map<List<CheckTypeDTO>>(types);
        }

        [HttpGet("settings")]
        public async Task<ISettings> GetSettings()
        {
            return await _settingsHelper.Get();
        }

        [HttpPost]
        public async Task<CheckDetailDTO> Edit([FromBody]CheckDetailDTO value)
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
            return await GetDetails(check.ID);
        }

        [HttpPost("settings")]
        public async Task<ISettings> SetSettings([FromBody]Settings value)
        {
            var global = await _settingsHelper.GetGlobal();
            await _settingsHelper.Set(value);
            if (global.TimeZoneId != value.Global.TimeZoneId)
            {
                _logger.LogInformation($"TimeZoneId changed from {global.TimeZoneId} to {value.Global.TimeZoneId}, updating schedules");
                await _manager.UpdateSchedules();
            }
            await _manager.UpdateMaintenanceJobs(value.Global);
            return await GetSettings();
        }

        [HttpPost("run/{id:int}")]
        public async Task<List<RunLog>> Run(int id)
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
        public async Task<bool> Delete(int id)
        {
            var check = await _checks.GetDetails(id, true);
            _checks.Delete(check);
            await _manager.RemoveSchedule(check);
            await _checks.SaveChangesAsync();
            return true;
        }

        [HttpGet("subchecktypes/{checkTypeID:int}")]
        public async Task<List<SubCheckTypeDTO>> GetSubCheckTypes(int checkTypeID)
        {
            var subCheckTypes = await _subCheckTypes.GetAll().Where(x => x.CheckTypeID == checkTypeID).ToListAsync();
            return _mapper.Map<List<SubCheckTypeDTO>>(subCheckTypes);
        }

        [HttpGet("checknotificationtypes")]
        public async Task<List<CheckNotificationTypeDTO>> GetCheckNotificationTypes()
        {
            var checkNotificationTypes = await _checkNotificationTypes.GetAll().ToListAsync();
            return _mapper.Map<List<CheckNotificationTypeDTO>>(checkNotificationTypes);
        }

        [HttpGet("slackchannels")]
        public async Task<Channel[]> GetSlackChannels()
        {
            return await _slackHelper.GetChannels();
        }

        [HttpGet("contacttypes")]
        public async Task<List<ContactTypeDTO>> GetContactTypes()
        {
            var contactTypes = await _contactTypes.GetAll().ToListAsync();
            return _mapper.Map<List<ContactTypeDTO>>(contactTypes);
        }

        [HttpGet("triggers")]
        public async Task<List<ITrigger>> GetAllTriggers()
        {
            var triggers = await _manager.GetAllTriggers();
            return triggers;
        }

        [HttpPost("login")]
        public async Task<LoginResult> Login([FromBody] LoginRequest request)
        {
            return await _securityHelper.Login(request, HttpContext);
        }

        [HttpGet("user")]
        public async Task<UserDTO> GetUser()
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
        public async Task<UserDTO> EditUser([FromBody]UserDTO value)
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
            return await GetUser();
        }

        [HttpGet("init")]
        public async Task<InitResult> GetInit()
        {
            var result = new InitResult();

            var anyUsers = await _users.GetAll().AnyAsync(x => !x.IsWindowsUser);
            result.Required = !anyUsers;

            return result;
        }

        [HttpPost("init")]
        public async Task<InitResult> SetInit([FromBody]InitRequest request)
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
        public async Task<object> ValidateCronExpression(bool? validateOnly, [FromBody]string cron)
        {
            try
            {
                var expression = new CronExpression(cron)
                {
                    TimeZone = await _jobHelper.GetTimeZone()
                };
                if (validateOnly ?? false)
                {
                    return new
                    {
                        valid = true
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
                return new
                {
                    valid = true,
                    next
                };
            }
            catch (Exception e)
            {
                return new
                {
                    valid = false,
                    error = e.Message
                };
            }
        }
    }
}
