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

namespace SystemChecker.Web.Controllers
{
    [Route("api")]
    public class APIController : Controller
    {
        private readonly ICheckerUow _uow;
        private readonly IMapper _mapper;
        private readonly ISchedulerManager _manager;
        private readonly ISettingsHelper _settingsHelper;
        public APIController(ICheckerUow uow, IMapper mapper, ISchedulerManager manager, ISettingsHelper settingsHelper)
        {
            _uow = uow;
            _mapper = mapper;
            _manager = manager;
            _settingsHelper = settingsHelper;
        }

        [HttpGet]
        public async Task<List<CheckDTO>> GetAll()
        {
            var checks = await _uow.Checks.GetAll().ToListAsync();
            var dtos = _mapper.Map<List<CheckDTO>>(checks);
            foreach (var check in dtos)
            {
                var result = await _uow.CheckResults.GetAll().Where(x => x.CheckID == check.ID).OrderByDescending(x => x.ID).FirstOrDefaultAsync();
                check.LastResultStatus = result?.Status;
            }
            return dtos;
        }

        [HttpGet("{id:int}")]
        public async Task<CheckDetailDTO> GetDetails(int id)
        {
            var check = await _uow.Checks.GetDetails(id);
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
            return await _manager.RunCheck(check);
        }

        [HttpDelete("{id:int}")]
        public async Task<bool> Delete(int id)
        {
            var check = await _uow.Checks.GetDetails(id);
            _uow.Checks.Delete(check);
            await _manager.RemoveSchedule(check);
            await _uow.Commit();
            return true;
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
