// ReSharper disable ClassNeverInstantiated.Global
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using AzureStorage.Tables;
using Common.Log;
using Lykke.blue.Service.ReferralLinks.Core;
using Lykke.blue.Service.ReferralLinks.Core.Services;
using Lykke.blue.Service.ReferralLinks.Core.Settings;
using Lykke.blue.Service.ReferralLinks.Models;
using Lykke.blue.Service.ReferralLinks.Modules;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.Common.ApiLibrary.Swagger;
using Lykke.Logs;
using Lykke.SettingsReader;
using Lykke.SlackNotification.AzureQueue;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Converters;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace Lykke.blue.Service.ReferralLinks
{
    public class Startup
    {
        private IHostingEnvironment Environment { get; }
        private IContainer ApplicationContainer { get; set; }
        private IConfigurationRoot Configuration { get; }
        private ILog Log { get; set; }

        private static Timer _timer;

        public Startup(IHostingEnvironment env)
        {
            Mapper.Initialize(cfg =>
            {
                cfg.AddProfiles(typeof(AutoMapperProfile));
                cfg.AddProfiles(typeof(AzureRepositories.AutoMapperProfile));
                cfg.AddProfiles(typeof(Services.AutoMapperProfile));
            });

            Mapper.AssertConfigurationIsValid();

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            Environment = env;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            try
            {
                services.AddMvc()
                    .AddJsonOptions(options =>
                    {
                        options.SerializerSettings.Converters.Add(new StringEnumConverter());
                        options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                    });

                services.AddSwaggerGen(options =>
                {
                    options.DefaultLykkeConfiguration("v1", Constants.ComponentName);
                });

                var builder = new ContainerBuilder();
                var appSettings = Configuration.LoadSettings<AppSettings>();
                Log = CreateLogWithSlack(services, appSettings);

                builder.RegisterModule(new ServiceModule(appSettings, Log));
                builder.Populate(services);
                ApplicationContainer = builder.Build();

                return new AutofacServiceProvider(ApplicationContainer);
            }
            catch (Exception ex)
            {
                Log?.WriteFatalErrorAsync(nameof(Startup), nameof(ConfigureServices), "", ex).Wait();
                throw;
            }
        }

        // ReSharper disable once UnusedMember.Global
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            try
            {
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }

                app.UseLykkeMiddleware(Constants.ComponentName, ex =>
                {
                    string errorMessage = "Technical problem";

                    if (!String.IsNullOrWhiteSpace(ex.Message))
                    {
                        errorMessage = ex.Message;
                    }

                    return ErrorResponseModel.Create(errorMessage);
                });

                app.UseMvc();
                app.UseSwagger();
                app.UseSwaggerUI(x =>
                {
                    x.RoutePrefix = "swagger/ui";
                    x.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                });
                app.UseStaticFiles();

                appLifetime.ApplicationStarted.Register(() => StartApplication().Wait());
                appLifetime.ApplicationStopped.Register(() => CleanUp().Wait());
            }
            catch (Exception ex)
            {
                Log?.WriteFatalErrorAsync(nameof(Startup), nameof(ConfigureServices), "", ex).Wait();
                throw;
            }
        }

        private async Task StartApplication()
        {
            try
            {
                await Log.WriteMonitorAsync("Referral Links Service", "Startup", "Service Started");

                var appSettings = Configuration.LoadSettings<AppSettings>();
                ConfigureExpiredLinksTimer(appSettings);
            }
            catch (Exception ex)
            {
                await Log.WriteFatalErrorAsync(nameof(Startup), nameof(StartApplication), "", ex);
                throw;
            }
        }

        private void ConfigureExpiredLinksTimer(IReloadingManager<AppSettings> settings)
        {
            _timer = new Timer(settings.CurrentValue.ReferralLinksService.GiftCoinsLinkSettings.ExpiredLinksCheckTimeoutMinutes * 60000);
            _timer.Elapsed += ReturnCoinsToSenderForExpiredGiftCoins;
            _timer.AutoReset = true;
            _timer.Enabled = true;
            _timer.Start();
        }

        private async void ReturnCoinsToSenderForExpiredGiftCoins(Object source, ElapsedEventArgs e)
        {
            _timer.Enabled = false;
            var referralLinksService = ApplicationContainer.Resolve<IReferralLinksService>();
            await referralLinksService.CheckForExpiredGiftCoinLink(); 
            _timer.Enabled = true;
        }        

        private async Task CleanUp()
        {
            try
            {
                // NOTE: Service can't receive and process requests here, so you can destroy all resources
                
                if (Log != null)
                {
                    await Log.WriteMonitorAsync("", "", "Terminating");
                }
                
                ApplicationContainer.Dispose();
            }
            catch (Exception ex)
            {
                if (Log != null)
                {
                    await Log.WriteFatalErrorAsync(nameof(Startup), nameof(CleanUp), "", ex);
                    (Log as IDisposable)?.Dispose();
                }
                throw;
            }
        }

        private static ILog CreateLogWithSlack(IServiceCollection services, IReloadingManager<AppSettings> settings)
        {
            var consoleLogger = new LogToConsole();
            var aggregateLogger = new AggregateLogger();

            aggregateLogger.AddLog(consoleLogger);

            // Creating slack notification service, which logs own azure queue processing messages to aggregate log
            var slackService = services.UseSlackNotificationsSenderViaAzureQueue(new AzureQueueIntegration.AzureQueueSettings
            {
                ConnectionString = settings.CurrentValue.SlackNotifications.AzureQueue.ConnectionString,
                QueueName = settings.CurrentValue.SlackNotifications.AzureQueue.QueueName
            }, aggregateLogger);

            var dbLogConnectionStringManager = settings.Nested(x => x.ReferralLinksService.Db.LogsConnString);
            var dbLogConnectionString = dbLogConnectionStringManager.CurrentValue;

            // Creating azure storage logger, which logs own messages to console log
            if (!string.IsNullOrEmpty(dbLogConnectionString) && !(dbLogConnectionString.StartsWith("${") && dbLogConnectionString.EndsWith("}")))
            {
                var persistenceManager = new LykkeLogToAzureStoragePersistenceManager(
                    AzureTableStorage<LogEntity>.Create(dbLogConnectionStringManager, "ReferralLinksLog", consoleLogger),
                    consoleLogger); 

                 var slackNotificationsManager = new LykkeLogToAzureSlackNotificationsManager(slackService, consoleLogger);

                var azureStorageLogger = new LykkeLogToAzureStorage(
                    persistenceManager,
                    slackNotificationsManager,
                    consoleLogger);

                azureStorageLogger.Start();

                aggregateLogger.AddLog(azureStorageLogger);
            }

            return aggregateLogger;
        }
    }
}
