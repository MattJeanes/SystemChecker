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
using SystemChecker.Model.Notifiers;

namespace SystemChecker.Model.Helpers
{
    public interface ICheckerHelper
    {
        Task<ISettings> GetSettings();
        Task SaveResult(CheckResult result);
        Task RunSubChecks(Check check, ICheckLogger logger, Action<SubCheck> action);
        Task RunNotifiers(Check check, ICheckLogger logger);
    }
    public class CheckerHelper : ICheckerHelper
    {
        private readonly ICheckerUow _uow;
        private readonly ISettingsHelper _settingsHelper;
        private readonly IHubContext<DashboardHub> _dashboardHub;
        private readonly IHubContext<DetailsHub> _detailsHub;
        private readonly IServiceProvider _serviceProvider;
        public CheckerHelper(ICheckerUow uow, IMapper mapper, ISettingsHelper settingsHelper, IHubContext<DashboardHub> dashboardHub,
            IHubContext<DetailsHub> detailsHub, IServiceProvider serviceProvider)
        {
            _uow = uow;
            _settingsHelper = settingsHelper;
            _dashboardHub = dashboardHub;
            _detailsHub = detailsHub;
            _serviceProvider = serviceProvider;
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
            await _dashboardHub.Clients.All.InvokeAsync("check", result.CheckID);
            await _detailsHub.Clients.All.InvokeAsync("check", result.CheckID);
        }

        public async Task RunNotifiers(Check check, ICheckLogger logger)
        {
            if (!check.Notifications.Any())
            {
                logger.Info("No notifications configured");
                return;
            }
            logger.Info("Running notifiers");
            foreach (var notification in check.Notifications)
            {
                if (!Enum.IsDefined(typeof(Enums.CheckNotificationType), notification.TypeID))
                {
                    logger.Warn($"Unknown notification type: {notification.TypeID} - ignoring");
                    continue;
                }
                var notifier = GetNotifier((Enums.CheckNotificationType)notification.TypeID);
                await notifier.Run(check, notification, logger);
            }
        }

        private BaseNotifier GetNotifier(Enums.CheckNotificationType notificationType)
        {
            Type type;
            switch (notificationType)
            {
                case Enums.CheckNotificationType.Slack:
                    type = typeof(SlackNotifier);
                    break;
                default:
                    throw new Exception($"Invalid notification type: {notificationType}");
            }
            return _serviceProvider.GetService(type) as BaseNotifier;
        }
    }
}