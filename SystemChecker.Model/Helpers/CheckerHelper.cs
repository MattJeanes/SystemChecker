using AutoMapper;
using Microsoft.AspNetCore.SignalR;
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
using SystemChecker.Model.Hubs;
using SystemChecker.Model.Loggers;

namespace SystemChecker.Model.Helpers
{
    public interface ICheckerHelper
    {
        Task<ISettings> GetSettings();
        Task SaveResult(CheckResult result);
        Task RunSubChecks(Check check, ICheckLogger logger, Action<SubCheck> action);
    }
    public class CheckerHelper : ICheckerHelper
    {
        private readonly ICheckerUow _uow;
        private readonly ISettingsHelper _settingsHelper;
        private readonly IHubContext<DashboardHub> _hubContext;
        public CheckerHelper(ICheckerUow uow, IMapper mapper, ISettingsHelper settingsHelper, IHubContext<DashboardHub> hubContext)
        {
            _uow = uow;
            _settingsHelper = settingsHelper;
            _hubContext = hubContext;
        }

        public async Task<ISettings> GetSettings()
        {
            return await _settingsHelper.Get();
        }

        public async Task RunSubChecks(Check check, ICheckLogger logger, Action<SubCheck> action)
        {
            var types = await _uow.SubCheckTypes.GetAll().Where(x => x.CheckTypeID == check.TypeID).ToListAsync();
            var subChecks = check.SubChecks.Where(x => x.Active);
            foreach (var subCheck in subChecks)
            {
                var type = types.FirstOrDefault(x => x.ID == subCheck.TypeID) ?? throw new Exception($"Invalid type {subCheck.TypeID}");
                logger.Info($"Running {type.Name} sub-check");
                action(subCheck);
            }
        }

        public async Task SaveResult(CheckResult result)
        {
            _uow.CheckResults.Add(result);
            await _uow.Commit();
            await _hubContext.Clients.All.InvokeAsync("check");
        }
    }
}
