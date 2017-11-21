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
        private readonly IEncryptionHelper _encryptionHelper;
        public APIController(ICheckerUow uow, IMapper mapper, ISchedulerManager manager, IEncryptionHelper encryptionHelper)
        {
            _uow = uow;
            _mapper = mapper;
            _manager = manager;
            _encryptionHelper = encryptionHelper;
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
        public async Task<ISettings> GetSettings()
        {
            return await _manager.GetSettings();
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

            if (check.Data != null)
            {
                _uow.CheckData.Update(check.Data);
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
            await _manager.UpdateSchedule(check);
            return await GetDetails(check.ID);
        }

        [HttpPost("settings")]
        public async Task<ISettings> SetSettings([FromBody]Settings value)
        {
            var logins = await _uow.Logins.GetAll().ToListAsync();
            foreach (var login in logins)
            {
                if (!value.Logins.Exists(x => x.ID == login.ID))
                {
                    _uow.Logins.Delete(login);
                }
                else
                {
                    _uow.Logins.Detach(login);
                }
            }
            _mapper.Map(value.Logins, logins);
            foreach (var login in logins)
            {
                login.Password = _encryptionHelper.Encrypt(login.Password);
                if (login.ID > 0)
                {
                    _uow.Logins.Update(login);
                }
                else
                {
                    _uow.Logins.Add(login);
                }
            }

            var connStrings = await _uow.ConnStrings.GetAll().ToListAsync();
            foreach (var connString in connStrings)
            {
                if (!value.ConnStrings.Exists(x => x.ID == connString.ID))
                {
                    _uow.ConnStrings.Delete(connString);
                }
                else
                {
                    _uow.ConnStrings.Detach(connString);
                }
            }
            _mapper.Map(value.ConnStrings, connStrings);
            foreach (var connString in connStrings)
            {
                connString.Value = _encryptionHelper.Encrypt(connString.Value);
                if (connString.ID > 0)
                {
                    _uow.ConnStrings.Update(connString);
                }
                else
                {
                    _uow.ConnStrings.Add(connString);
                }
            }

            await _uow.Commit();
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
            foreach (var schedule in check.Schedules)
            {
                _uow.CheckSchedules.Delete(schedule);
            }
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
