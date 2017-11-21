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
    public interface ICheckerHelper
    {
        Task<ISettings> GetSettings();
        Task SaveResult(CheckResult result);
    }
    public class CheckerHelper : ICheckerHelper
    {
        private readonly ICheckerUow _uow;
        private readonly ISettingsHelper _settingsHelper;
        public CheckerHelper(ICheckerUow uow, IMapper mapper, ISettingsHelper settingsHelper)
        {
            _uow = uow;
            _settingsHelper = settingsHelper;
        }

        public async Task<ISettings> GetSettings()
        {
            return await _settingsHelper.Get();
        }
        
        public async Task SaveResult(CheckResult result)
        {
            _uow.CheckResults.Add(result);
            await _uow.Commit();
        }
    }
}
