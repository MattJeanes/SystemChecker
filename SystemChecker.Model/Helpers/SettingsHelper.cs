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
        private readonly ICheckerUow _uow;
        private readonly IMapper _mapper;
        private readonly IEncryptionHelper _encryptionHelper;
        public SettingsHelper(ICheckerUow uow, IMapper mapper, IEncryptionHelper encryptionHelper)
        {
            _uow = uow;
            _mapper = mapper;
            _encryptionHelper = encryptionHelper;
        }
        public async Task<ISettings> Get()
        {
            var logins = await _uow.Logins.GetAll().ToListAsync();
            var connStrings = await _uow.ConnStrings.GetAll().ToListAsync();
            var environments = await _uow.Environments.GetAll().ToListAsync();
            var contacts = await _uow.Contacts.GetAll().ToListAsync();

            var settings = new Settings
            {
                Logins = _mapper.Map<List<LoginDTO>>(logins),
                ConnStrings = _mapper.Map<List<ConnStringDTO>>(connStrings),
                Environments = _mapper.Map<List<EnvironmentDTO>>(environments),
                Contacts = _mapper.Map<List<ContactDTO>>(contacts),
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
            var connStrings = await _uow.ConnStrings.GetAll().ToListAsync();
            foreach (var connString in connStrings.Where(x => !settings.ConnStrings.Exists(y => y.ID == x.ID)))
            {
                _uow.ConnStrings.Delete(connString);
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
                    _uow.ConnStrings.Add(connString);
                }
            }

            var logins = await _uow.Logins.GetAll().ToListAsync();
            foreach (var login in logins.Where(x => !settings.Logins.Exists(y => y.ID == x.ID)))
            {
                _uow.Logins.Delete(login);
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
                    _uow.Logins.Add(login);
                }
            }

            var environments = await _uow.Environments.GetAll().ToListAsync();
            foreach (var environment in environments.Where(x => !settings.Environments.Exists(y => y.ID == x.ID)))
            {
                _uow.Environments.Delete(environment);
            }
            _mapper.Map(settings.Environments, environments);
            foreach (var environment in environments)
            {
                if (environment.ID == 0)
                {
                    _uow.Environments.Add(environment);
                }
            }

            var contacts = await _uow.Contacts.GetAll().ToListAsync();
            foreach (var contact in contacts.Where(x => !settings.Contacts.Exists(y => y.ID == x.ID)))
            {
                _uow.Contacts.Delete(contact);
            }
            _mapper.Map(settings.Contacts, contacts);
            foreach (var contact in contacts)
            {
                if (contact.ID == 0)
                {
                    _uow.Contacts.Add(contact);
                }
            }

            await _uow.Commit();
        }
    }
}
