using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using Lykke.Service.Kyc.Abstractions.Services;
using Lykke.Service.Kyc.Client;
using Lykke.Service.ReferralLinks.Core.Services;
using Lykke.Service.ReferralLinks.Core.Settings.ServiceSettings;
using Lykke.Service.ReferralLinks.Services;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;
using Common;
using Lykke.Service.ReferralLinks.Core.Assets;
using System.Linq;
using Lykke.Service.Assets.Client;
using System;
using Lykke.Service.ReferralLinks.Models;
using Lykke.Service.ReferralLinks.AzureRepositories;
using Lykke.Service.ReferralLinks.Services.Kyc;
using Lykke.Service.ReferralLinks.Core.Kyc;
using Lykke.Service.ClientAccount.Client;

namespace Lykke.Service.ReferralLinks.Modules
{
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<ReferralLinksSettings> _settings;
        private readonly ILog _log;
        // NOTE: you can remove it if you don't need to use IServiceCollection extensions to register service specific dependencies
        private readonly IServiceCollection _services;

        public ServiceModule(IReloadingManager<ReferralLinksSettings> settings, ILog log)
        {
            _settings = settings;
            _log = log;

            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
            // TODO: Do not register entire settings in container, pass necessary settings to services which requires them
            // ex:
            //  builder.RegisterType<QuotesPublisher>()
            //      .As<IQuotesPublisher>()
            //      .WithParameter(TypedParameter.From(_settings.CurrentValue.QuotesPublication))

            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>();

            builder.BindAzureRepositories(_settings, _log);

            builder.RegisterType<Lykke.Service.Assets.Client.AssetsService>()
                .As<Lykke.Service.Assets.Client.IAssetsService>()
                .SingleInstance();

            builder.Register<IAssetsService>(x =>
            {
                var assetsSrv = new AssetsService(new Uri(_settings.CurrentValue.Services.AssetsServiceUrl));
                return assetsSrv;
            }).SingleInstance();

            builder.Register(c =>
            {
                var ctx = c.Resolve<IComponentContext>();
                return new CachedDataDictionary<string, Asset>(
                    async () =>
                        (await ctx.Resolve<Lykke.Service.Assets.Client.IAssetsService>().AssetGetAllWithHttpMessagesAsync()).Body.Select(a=>a.ConvertToServiceModel()).ToDictionary(itm => itm.Id));
            }).SingleInstance();

            builder.RegisterType<KycStatusServiceClient>().As<IKycStatusService>().SingleInstance();
            builder.RegisterType<SrvKycForAsset>().As<ISrvKycForAsset>().SingleInstance();

            builder.RegisterType<ReferralLinksService>().As<IReferralLinksService>().SingleInstance();
            builder.RegisterType<ClientAccountClient>()
                .As<IClientAccountClient>()
                .WithParameter("serviceUrl", _settings.CurrentValue.Services.ClientAccountServiceUrl)
                .WithParameter("log", _log);

            builder.Populate(_services);
        }
    }
}
