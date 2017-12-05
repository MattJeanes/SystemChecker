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

namespace SystemChecker.Web.Controllers
{
    [Route("api")]
    public class APIController : Controller
    {
        private readonly ICheckerUow _uow;
        private readonly IMapper _mapper;
        private readonly ISchedulerManager _manager;
        private readonly ISettingsHelper _settingsHelper;
        private readonly ISlackHelper _slackHelper;
        public APIController(ICheckerUow uow, IMapper mapper, ISchedulerManager manager, ISettingsHelper settingsHelper, ISlackHelper slackHelper)
        {
            _uow = uow;
            _mapper = mapper;
            _manager = manager;
            _settingsHelper = settingsHelper;
            _slackHelper = slackHelper;
        }

        [HttpGet]
        public async Task<List<CheckDTO>> GetAll()
        {
            var checks = await _uow.Checks.GetAll().ToListAsync();
            var dtos = _mapper.Map<List<CheckDTO>>(checks);
            foreach (var check in dtos)
            {
                var result = await _uow.CheckResults.GetAll().Where(x => x.CheckID == check.ID).OrderByDescending(x => x.ID).FirstOrDefaultAsync();
                var status = result?.Status;
                if (status == null)
                {
                    check.LastResultStatus = CheckResultStatus.NotRun;
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
            return dtos;
        }

        [HttpGet("{id:int}/{includeResults:bool?}")]
        public async Task<CheckDetailDTO> GetDetails(int id, bool? includeResults = null)
        {
            var check = await _uow.Checks.GetDetails(id, includeResults ?? false);
            return _mapper.Map<CheckDetailDTO>(check);
        }

        [HttpGet("types")]
        public async Task<List<CheckTypeDTO>> GetTypes()
        {
            var types = await _uow.CheckTypes.GetAll().ToListAsync();
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
                check = await _uow.Checks.GetDetails(value.ID);
            }
            else
            {
                check = new Check
                {
                    Schedules = new List<CheckSchedule>()
                };
                _uow.Checks.Add(check);
            }

            _mapper.Map(value, check);

            await _uow.Commit();
            await _manager.UpdateSchedule(check);
            return await GetDetails(check.ID);
        }

        [HttpPost("settings")]
        public async Task<ISettings> SetSettings([FromBody]Settings value)
        {
            await _settingsHelper.Set(value);
            return await GetSettings();
        }

        [HttpPost("run/{id:int}")]
        public async Task<List<RunLog>> Run(int id)
        {
            var check = await _uow.Checks.GetDetails(id);
            var logger = await _manager.RunManualUICheck(check);
            try
            {
                await _uow.Commit();
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
            var check = await _uow.Checks.GetDetails(id, true);
            _uow.Checks.Delete(check);
            await _manager.RemoveSchedule(check);
            await _uow.Commit();
            return true;
        }

        [HttpGet("subchecktypes/{checkTypeID:int}")]
        public async Task<List<SubCheckTypeDTO>> GetSubCheckTypes(int checkTypeID)
        {
            var subCheckTypes = await _uow.SubCheckTypes.GetAll().Where(x => x.CheckTypeID == checkTypeID).ToListAsync();
            return _mapper.Map<List<SubCheckTypeDTO>>(subCheckTypes);
        }

        [HttpGet("checknotificationtypes")]
        public async Task<List<CheckNotificationTypeDTO>> GetCheckNotificationTypes()
        {
            var checkNotificationTypes = await _uow.CheckNotificationTypes.GetAll().ToListAsync();
            return _mapper.Map<List<CheckNotificationTypeDTO>>(checkNotificationTypes);
        }

        [HttpGet("slackchannels")]
        public async Task<Channel[]> GetSlackChannels()
        {
            return await _slackHelper.GetChannels();
        }

        [HttpPost("cron/{validateOnly:bool?}")]
        public object ValidateCronExpression(bool? validateOnly, [FromBody]string cron)
        {
            try
            {
                var expression = new CronExpression(cron);
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
                    next += time.Value.DateTime.ToString("yyyy-mm-dd HH:mm:ss") + "\n";
                }
                next += "\n" + expression.GetExpressionSummary();
                return new
                {
                    valid = true,
                    next = next
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
