using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SystemChecker.Model.Data;
using SystemChecker.Model.Data.Entities;
using SystemChecker.Model.Data.Interfaces;
using SystemChecker.Model.DTO;

namespace SystemChecker.Model.Helpers
{
    public interface ISettingsHelper
    {
        Task<ISettings> Get();
        Task<GlobalSettings> GetGlobal();
        Task Set(ISettings settings);
        Task SetGlobal(GlobalSettings settings);
    }
    public class SettingsHelper : ISettingsHelper
    {
        private readonly IRepository<Login> _logins;
        private readonly IRepository<ConnString> _connStrings;
        private readonly IRepository<Data.Entities.Environment> _environments;
        private readonly IRepository<Contact> _contacts;
        private readonly IRepository<CheckGroup> _checkGroups;
        private readonly IRepository<GlobalSetting> _globalSettings;
        private readonly AppSettings _appSettings;
        private readonly IMapper _mapper;
        public SettingsHelper(IRepository<Login> logins, IRepository<ConnString> connStrings, IRepository<Data.Entities.Environment> environments,
            IRepository<Contact> contacts, IRepository<CheckGroup> checkGroups, IRepository<GlobalSetting> globalSettings, IOptions<AppSettings> appSettings, IMapper mapper)
        {
            _logins = logins;
            _connStrings = connStrings;
            _environments = environments;
            _contacts = contacts;
            _checkGroups = checkGroups;
            _globalSettings = globalSettings;
            _appSettings = appSettings.Value;
            _mapper = mapper;
        }
        public async Task<ISettings> Get()
        {
            var logins = await _logins.GetAll().ToListAsync();
            var connStrings = await _connStrings.GetAll().ToListAsync();
            var environments = await _environments.GetAll().ToListAsync();
            var contacts = await _contacts.GetAll().ToListAsync();
            var checkGroups = await _checkGroups.GetAll().ToListAsync();

            var settings = new Settings
            {
                Logins = _mapper.Map<List<LoginDTO>>(logins),
                ConnStrings = _mapper.Map<List<ConnStringDTO>>(connStrings),
                Environments = _mapper.Map<List<EnvironmentDTO>>(environments),
                Contacts = _mapper.Map<List<ContactDTO>>(contacts),
                CheckGroups = _mapper.Map<List<CheckGroupDTO>>(checkGroups),
                Global = await GetGlobal()
            };

            return settings;
        }

        public async Task Set(ISettings settings)
        {
            var connStrings = await _connStrings.GetAll().ToListAsync();
            foreach (var connString in connStrings.Where(x => !settings.ConnStrings.Exists(y => y.ID == x.ID)))
            {
                _connStrings.Delete(connString);
            }
            _mapper.Map(settings.ConnStrings, connStrings);
            foreach (var connString in connStrings)
            {
                if (connString.ID == 0)
                {
                    _connStrings.Add(connString);
                }
            }

            var logins = await _logins.GetAll().ToListAsync();
            foreach (var login in logins.Where(x => !settings.Logins.Exists(y => y.ID == x.ID)))
            {
                _logins.Delete(login);
            }
            _mapper.Map(settings.Logins, logins);
            foreach (var login in logins)
            {
                if (login.ID == 0)
                {
                    _logins.Add(login);
                }
            }

            var environments = await _environments.GetAll().ToListAsync();
            foreach (var environment in environments.Where(x => !settings.Environments.Exists(y => y.ID == x.ID)))
            {
                _environments.Delete(environment);
            }
            _mapper.Map(settings.Environments, environments);
            foreach (var environment in environments)
            {
                if (environment.ID == 0)
                {
                    _environments.Add(environment);
                }
            }

            var contacts = await _contacts.GetAll().ToListAsync();
            foreach (var contact in contacts.Where(x => !settings.Contacts.Exists(y => y.ID == x.ID)))
            {
                _contacts.Delete(contact);
            }
            _mapper.Map(settings.Contacts, contacts);
            foreach (var contact in contacts)
            {
                if (contact.ID == 0)
                {
                    _contacts.Add(contact);
                }
            }

            var checkGroups = await _checkGroups.GetAll().ToListAsync();
            foreach (var group in checkGroups.Where(x => !settings.CheckGroups.Exists(y => y.ID == x.ID)))
            {
                _checkGroups.Delete(group);
            }
            _mapper.Map(settings.CheckGroups, checkGroups);
            foreach (var group in checkGroups)
            {
                if (group.ID == 0)
                {
                    _checkGroups.Add(group);
                }
            }

            await SetGlobal(settings.Global);
            await _logins.SaveChangesAsync();
            await _connStrings.SaveChangesAsync();
            await _environments.SaveChangesAsync();
            await _contacts.SaveChangesAsync();
            await _checkGroups.SaveChangesAsync();
        }

        public async Task<GlobalSettings> GetGlobal()
        {
            var global = await _globalSettings.GetAll().ToListAsync();

            var clickatell = await _globalSettings.Find("Clickatell");
            var email = await _globalSettings.Find("Email");
            var authenticationGroup = await _globalSettings.Find("AuthenticationGroup");
            var slackToken = await _globalSettings.Find("SlackToken");
            var url = await _globalSettings.Find("Url");
            var resultRetentionMonths = await _globalSettings.Find("ResultRetentionMonths");
            var resultAggregateDays = await _globalSettings.Find("ResultAggregateDays");
            var cleanupSchedule = await _globalSettings.Find("CleanupSchedule");
            var loginExpireAfterDays = await _globalSettings.Find("LoginExpireAfterDays");

            return new GlobalSettings
            {
                Clickatell = clickatell != null ? JsonConvert.DeserializeObject<ClickatellSettings>(clickatell.Value) : null,
                Email = email != null ? JsonConvert.DeserializeObject<EmailSettings>(email.Value) : null,
                AuthenticationGroup = authenticationGroup != null ? JsonConvert.DeserializeObject<string>(authenticationGroup.Value) : null,
                SlackToken = slackToken != null ? JsonConvert.DeserializeObject<string>(slackToken.Value) : null,
                Url = string.IsNullOrEmpty(_appSettings.Url) && url != null ? JsonConvert.DeserializeObject<string>(url.Value) : _appSettings.Url,
                ResultRetentionMonths = JsonConvert.DeserializeObject<int?>(resultRetentionMonths.Value),
                ResultAggregateDays = JsonConvert.DeserializeObject<int?>(resultAggregateDays.Value),
                CleanupSchedule = JsonConvert.DeserializeObject<string>(cleanupSchedule.Value),
                LoginExpireAfterDays = JsonConvert.DeserializeObject<int>(loginExpireAfterDays.Value),
            };
        }

        public async Task SetGlobal(GlobalSettings global)
        {
            if (global.LoginExpireAfterDays <= 0)
            {
                throw new Exception($"{nameof(global.LoginExpireAfterDays)} must be greater than zero");
            }
            ((await _globalSettings.Find("Clickatell")) ?? _globalSettings.Add(new GlobalSetting { Key = "Clickatell" })).Value = JsonConvert.SerializeObject(global.Clickatell);
            ((await _globalSettings.Find("Email")) ?? _globalSettings.Add(new GlobalSetting { Key = "Email" })).Value = JsonConvert.SerializeObject(global.Email);
            ((await _globalSettings.Find("AuthenticationGroup")) ?? _globalSettings.Add(new GlobalSetting { Key = "AuthenticationGroup" })).Value = JsonConvert.SerializeObject(global.AuthenticationGroup);
            ((await _globalSettings.Find("SlackToken")) ?? _globalSettings.Add(new GlobalSetting { Key = "SlackToken" })).Value = JsonConvert.SerializeObject(global.SlackToken);
            ((await _globalSettings.Find("Url")) ?? _globalSettings.Add(new GlobalSetting { Key = "Url" })).Value = JsonConvert.SerializeObject(global.Url);
            ((await _globalSettings.Find("ResultRetentionMonths")) ?? _globalSettings.Add(new GlobalSetting { Key = "ResultRetentionMonths" })).Value = JsonConvert.SerializeObject(global.ResultRetentionMonths);
            ((await _globalSettings.Find("ResultAggregateDays")) ?? _globalSettings.Add(new GlobalSetting { Key = "ResultAggregateDays" })).Value = JsonConvert.SerializeObject(global.ResultAggregateDays);
            ((await _globalSettings.Find("CleanupSchedule")) ?? _globalSettings.Add(new GlobalSetting { Key = "CleanupSchedule" })).Value = JsonConvert.SerializeObject(global.CleanupSchedule);
            ((await _globalSettings.Find("LoginExpireAfterDays")) ?? _globalSettings.Add(new GlobalSetting { Key = "LoginExpireAfterDays" })).Value = JsonConvert.SerializeObject(global.LoginExpireAfterDays);
            await _globalSettings.SaveChangesAsync();
        }
    }
}
