using System;
using System.Collections.Generic;
using System.Text;
using SystemChecker.Model.Data;
using SystemChecker.Model.Data.Interfaces;
using Microsoft.Extensions;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Quartz;
using Quartz.Impl;
using SystemChecker.Model;
using Quartz.Spi;
using SystemChecker.Model.Jobs;
using SystemChecker.Model.Helpers;
using SystemChecker.Model.Loggers;
using AutoMapper.EquivalencyExpression;
using SystemChecker.Model.Notifiers;
using SystemChecker.Model.Data.Repositories;
using System.Collections.Specialized;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace SystemChecker.Model
{
    public static class DependencyInjection
    {
        public static void AddSystemChecker(this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddOptions();
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

            // Database
            var connString = Configuration.GetConnectionString("SystemChecker");
            if (string.IsNullOrEmpty(connString))
            {
                throw new ArgumentException("ConnectionStrings:SystemChecker option not set");
            }
            services.AddDbContext<CheckerContext>(options =>
            options.UseSqlServer(connString));

            var builder = new DbContextOptionsBuilder<CheckerContext>();
            builder.UseSqlServer(connString);
            services.AddTransient<ICheckerContext>(_ => new CheckerContext(builder.Options));
            services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
            services.AddSingleton<IMapper>(_ => new Mapper(new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
                cfg.AddCollectionMappers();
            })));

            // Repositories
            services.AddTransient<ICheckRepository, CheckRepository>();
            services.AddTransient<ICheckTypeRepository, CheckTypeRepository>();
            services.AddTransient<ISubCheckTypeRepository, SubCheckTypeRepository>();
            services.AddTransient<ICheckNotificationTypeRepository, CheckNotificationTypeRepository>();
            services.AddTransient<IUserRepository, UserRepository>();

            // Other
            var properties = new NameValueCollection
            {
                ["quartz.serializer.type"] = "json",
                ["quartz.jobStore.type"] = "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz",
                ["quartz.jobStore.useProperties"] = "false",
                ["quartz.jobStore.dataSource"] = "default",
                ["quartz.jobStore.clustered"] = "true",
                ["quartz.scheduler.instanceId"] = "AUTO",
                ["quartz.jobStore.tablePrefix"] = "QRTZ_",
                ["quartz.jobStore.driverDelegateType"] = "Quartz.Impl.AdoJobStore.SqlServerDelegate, Quartz",
                ["quartz.dataSource.default.provider"] = "SqlServer",
                ["quartz.dataSource.default.connectionString"] = connString
            };
            services.AddTransient<ISchedulerFactory>(_ => new StdSchedulerFactory(properties));
            services.AddSingleton<ISchedulerManager, SchedulerManager>();
            services.AddTransient<IJobFactory, JobFactory>();
            services.AddTransient<ICheckLogger, CheckLogger>();

            // Helpers
            services.AddTransient<ISettingsHelper, SettingsHelper>();
            services.AddTransient<ICheckerHelper, CheckerHelper>();
            services.AddTransient<ISlackHelper, SlackHelper>();
            services.AddTransient<ISMSHelper, SMSHelper>();
            services.AddTransient<IJobHelper, JobHelper>();
            services.AddTransient<ISecurityHelper, SecurityHelper>();

            // Jobs
            services.AddTransient<DatabaseChecker>();
            services.AddTransient<WebRequestChecker>();
            services.AddTransient<PingChecker>();
            services.AddTransient<CleanupJob>();

            // Notifiers
            services.AddTransient<SlackNotifier>();
            services.AddTransient<EmailNotifier>();
            services.AddTransient<SMSNotifier>();

            // Redis
            var redisUrl = Configuration.GetValue<string>("RedisUrl");
            var redis = ConnectionMultiplexer.Connect(redisUrl);
            services.AddSingleton<IConnectionMultiplexer>(redis);
        }
    }
}
