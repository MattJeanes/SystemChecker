using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SystemChecker.Contracts.Data;
using SystemChecker.Contracts.Enums;
using SystemChecker.Model.Data.Entities;
using SystemChecker.Model.Data.Interfaces;
using SystemChecker.Model.Loggers;
using SystemChecker.Model.Notifiers;

namespace SystemChecker.Model.Helpers
{
    public interface ICheckerHelper
    {
        Task<CheckerSettings> GetSettings();
        ICheckLogger GetCheckLogger();
        Task SaveResult(CheckResult result);
        Task RunSubChecks(Check check, ICheckLogger logger, Action<SubCheck> action);
        Task RunNotifiers(Check check, CheckResult result, CheckerSettings settings, ICheckLogger logger);
        Task<Check> GetDetails(int value);
        Task SaveChangesAsync();
        Task LoadResultStatuses();
        ResultStatus GetResultStatus(ResultStatusEnum resultStatus);
    }
    public class CheckerHelper : ICheckerHelper
    {
        private readonly IRepository<SubCheckType> _subCheckTypes;
        private readonly IRepository<CheckResult> _checkResults;
        private readonly ICheckRepository _checks;
        private readonly ISettingsHelper _settingsHelper;
        private readonly IServiceProvider _serviceProvider;
        private readonly ICheckLogger _checkLogger;
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly IRepository<ResultStatus> _resultStatuses;
        private List<ResultStatus> _resultStatusesCache;
        public CheckerHelper(
            IRepository<SubCheckType> subCheckTypes,
            IRepository<CheckResult> checkResults,
            ICheckRepository checks,
            ISettingsHelper settingsHelper,
            IServiceProvider serviceProvider,
            ICheckLogger checkLogger,
            IConnectionMultiplexer connectionMultiplexer,
            IRepository<ResultStatus> resultStatuses
            )
        {
            _subCheckTypes = subCheckTypes;
            _checkResults = checkResults;
            _checks = checks;
            _settingsHelper = settingsHelper;
            _serviceProvider = serviceProvider;
            _checkLogger = checkLogger;
            _connectionMultiplexer = connectionMultiplexer;
            _resultStatuses = resultStatuses;
        }

        public async Task<CheckerSettings> GetSettings()
        {
            return await _settingsHelper.Get();
        }

        public ICheckLogger GetCheckLogger()
        {
            return _checkLogger;
        }

        public async Task RunSubChecks(Check check, ICheckLogger logger, Action<SubCheck> action)
        {
            var types = await _subCheckTypes.GetAll().Where(x => x.CheckTypeID == check.TypeID).ToListAsync();
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
            var status = result.Status;
            result.StatusID = result.Status.ID;
            result.Status = null;
            _checkResults.Add(result);
            await _checkResults.SaveChangesAsync();
            result.Status = status;
            var pubsub = _connectionMultiplexer.GetSubscriber();
            await pubsub.PublishAsync("check", result.CheckID);
        }

        public async Task RunNotifiers(Check check, CheckResult result, CheckerSettings settings, ICheckLogger logger)
        {
            var notifications = check.Notifications.Where(x => x.Active);
            if (!notifications.Any())
            {
                logger.Info("No active notifications configured");
                return;
            }
            logger.Info("Running notifiers");
            foreach (var notification in notifications)
            {
                if (!Enum.IsDefined(typeof(Contracts.Enums.CheckNotificationType), notification.TypeID))
                {
                    logger.Warn($"Unknown notification type: {notification.TypeID} - ignoring");
                    continue;
                }
                var notifier = GetNotifier((Contracts.Enums.CheckNotificationType)notification.TypeID);
                await notifier.Run(check, notification, result, settings, logger);
            }
        }

        private BaseNotifier GetNotifier(Contracts.Enums.CheckNotificationType notificationType)
        {
            Type type;
            switch (notificationType)
            {
                case Contracts.Enums.CheckNotificationType.Slack:
                    type = typeof(SlackNotifier);
                    break;
                case Contracts.Enums.CheckNotificationType.Email:
                    type = typeof(EmailNotifier);
                    break;
                case Contracts.Enums.CheckNotificationType.SMS:
                    type = typeof(SMSNotifier);
                    break;
                default:
                    throw new Exception($"Invalid notification type: {notificationType}");
            }
            return _serviceProvider.GetService(type) as BaseNotifier;
        }

        public async Task<Check> GetDetails(int checkID)
        {
            return await _checks.GetDetails(checkID);
        }

        public async Task SaveChangesAsync()
        {
            await _checks.SaveChangesAsync();
        }

        public async Task LoadResultStatuses()
        {
            _resultStatusesCache = await _resultStatuses
                .GetAll()
                .Include(x => x.Type)
                .ToListAsync();
        }

        public ResultStatus GetResultStatus(ResultStatusEnum resultStatus)
        {
            if (_resultStatusesCache == null)
            {
                throw new Exception($"Result status cache not loaded call {nameof(LoadResultStatuses)} first");
            }
            return _resultStatusesCache
                .First(x => x.Identifier == resultStatus.ToString());
        }
    }
}