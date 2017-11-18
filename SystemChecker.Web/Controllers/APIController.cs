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

namespace SystemChecker.Web.Controllers
{
    [Route("api")]
    public class APIController : Controller
    {
        private readonly ICheckerUow _uow;
        private readonly IMapper _mapper;
        private readonly ISchedulerManager _manager;
        public APIController(ICheckerUow uow, IMapper mapper, ISchedulerManager manager)
        {
            _uow = uow;
            _mapper = mapper;
            _manager = manager;
        }

        [HttpGet]
        public async Task<List<CheckDTO>> GetAll()
        {
            var checks = await _uow.Checks.GetAll().ToListAsync();
            return _mapper.Map<List<CheckDTO>>(checks);
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
        public async Task<Settings> GetSettings()
        {
            var logins = await _uow.Logins.GetAll().ToListAsync();
            var connStrings = await _uow.ConnStrings.GetAll().ToListAsync();
            return new Settings
            {
                Logins = _mapper.Map<List<LoginDTO>>(logins),
                ConnStrings = _mapper.Map<List<ConnStringDTO>>(connStrings)
            };
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
                check = new Check();
            }
            foreach (var schedule in check.Schedules)
            {
                if (!value.Schedules.Exists(x => x.ID == schedule.ID))
                {
                    _uow.CheckSchedules.Delete(schedule);
                }
                else
                {
                    _uow.CheckSchedules.Detach(schedule);
                }
            }

            _mapper.Map(value, check);

            if (check.ID > 0)
            {
                _uow.Checks.Update(check);
            }
            else
            {
                _uow.Checks.Add(check);
            }
            foreach (var schedule in check.Schedules)
            {
                if (schedule.ID > 0)
                {
                    _uow.CheckSchedules.Update(schedule);
                }
                else
                {
                    _uow.CheckSchedules.Add(schedule);
                }
            }
            await _uow.Commit();
            _manager.UpdateSchedule(check);
            return await GetDetails(check.ID);
        }

        [HttpPost("settings")]
        public Settings SetSettings([FromBody]Settings value)
        {
            return value;
        }

        [HttpPost("run/{id:int}")]
        public bool StartRun(int id)
        {
            return true;
        }

        [HttpDelete("{id:int}")]
        public bool Delete(int id)
        {
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
