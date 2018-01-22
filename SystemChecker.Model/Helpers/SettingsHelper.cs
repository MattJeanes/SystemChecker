using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
        Task Set(ISettings settings);
    }
    public class SettingsHelper : ISettingsHelper
    {
        private readonly IRepository<Login> _logins;
        private readonly IRepository<ConnString> _connStrings;
        private readonly IRepository<Data.Entities.Environment> _environments;
        private readonly IRepository<Contact> _contacts;
        private readonly IRepository<CheckGroup> _checkGroups;
        private readonly IMapper _mapper;
        private readonly IEncryptionHelper _encryptionHelper;
        public SettingsHelper(IRepository<Login> logins, IRepository<ConnString> connStrings, IRepository<Data.Entities.Environment> environments, IRepository<Contact> contacts, IRepository<CheckGroup> checkGroups,
            IMapper mapper, IEncryptionHelper encryptionHelper)
        {
            _logins = logins;
            _connStrings = connStrings;
            _environments = environments;
            _contacts = contacts;
            _checkGroups = checkGroups;
            _mapper = mapper;
            _encryptionHelper = encryptionHelper;
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
            };

            foreach (var login in settings.Logins)
            {
                login.Password = _encryptionHelper.Decrypt(login.Password);
            }

            foreach (var connString in settings.ConnStrings)
            {
                connString.Value = _encryptionHelper.Decrypt(connString.Value);
            }

            return settings;
        }

        public async Task Set(ISettings settings)
        {
            var connStrings = await _connStrings.GetAll().ToListAsync();
            foreach (var connString in connStrings.Where(x => !settings.ConnStrings.Exists(y => y.ID == x.ID)))
            {
                _connStrings.Delete(connString);
            }
            foreach (var connString in settings.ConnStrings)
            {
                connString.Value = _encryptionHelper.Encrypt(connString.Value);
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
            foreach (var login in settings.Logins)
            {
                login.Password = _encryptionHelper.Encrypt(login.Password);
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

            await _logins.SaveChangesAsync();
            await _connStrings.SaveChangesAsync();
            await _environments.SaveChangesAsync();
            await _contacts.SaveChangesAsync();
            await _checkGroups.SaveChangesAsync();
        }
    }
}
