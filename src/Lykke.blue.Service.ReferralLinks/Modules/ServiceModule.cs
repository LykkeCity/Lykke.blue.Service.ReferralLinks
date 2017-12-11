using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common;
using Common.Log;
using Lykke.Bitcoin.Api.Client.BitcoinApi;
using Lykke.blue.Service.ReferralLinks.AzureRepositories;
using Lykke.blue.Service.ReferralLinks.Core.Kyc;
using Lykke.blue.Service.ReferralLinks.Core.Services;
using Lykke.blue.Service.ReferralLinks.Core.Settings;
using Lykke.blue.Service.ReferralLinks.Services;
using Lykke.blue.Service.ReferralLinks.Services.ExchangeOperations;
using Lykke.blue.Service.ReferralLinks.Services.Kyc;
using Lykke.blue.Service.ReferralLinks.Services.Offchain;
using Lykke.Service.Assets.Client;
using Lykke.Service.Balances.Client;
using Lykke.Service.ExchangeOperations.Client;
using Lykke.Service.Kyc.Abstractions.Services;
using Lykke.Service.Kyc.Client;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Lykke.blue.Service.ReferralLinks.Modules
{
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;
        private readonly ILog _log;
        private readonly IServiceCollection _services;

        public ServiceModule(IReloadingManager<AppSettings> settings, ILog log)
        {
            _settings = settings;
            _log = log;

            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
            RegisterLocalTypes(builder);
            RegisterRepos(builder);
            RegisterExternalServices(builder);
            RegisterLocalServices(builder);        
            
            RegisterDictionaryData(builder);

            builder.Populate(_services);
        }

        private void RegisterLocalTypes(ContainerBuilder builder)
        {
            builder.RegisterInstance(_log).As<ILog>().SingleInstance();
            builder.RegisterInstance(_settings.CurrentValue);
            builder.RegisterInstance(_settings.CurrentValue.ReferralLinksService);
        }

        private void RegisterLocalServices(ContainerBuilder builder)
        {
            builder.RegisterType<HealthService>().As<IHealthService>().SingleInstance();
            builder.RegisterType<SrvKycForAsset>().As<ISrvKycForAsset>().SingleInstance();
            
            builder.RegisterType<OffchainService>().As<IOffchainService>().SingleInstance();

            builder.RegisterType<ReferralLinksService>().As<IReferralLinksService>().SingleInstance();
            builder.RegisterType<ReferralLinkClaimsService>().As<IReferralLinkClaimsService>().SingleInstance();
            builder.RegisterType<StatisticsService>().As<IStatisticsService>().SingleInstance();

            builder.RegisterType<ExchangeService>().SingleInstance();
        }

        private void RegisterRepos(ContainerBuilder builder)
        {
            builder.BindAzureRepositories(_settings, _log);
        }

        private void RegisterExternalServices(ContainerBuilder builder)
        {
            builder.Register<IBitcoinApiClient>(x =>
            {
                var client = new BitcoinApiClient(_settings.CurrentValue.BitCoinCore.BitcoinCoreApiUrl);
                return client;
            }).SingleInstance();


            builder.Register<IKycStatusService>(x =>
            {
                var assetsSrv = new KycStatusServiceClient(_settings.CurrentValue.KycServiceClient, _log);
                return assetsSrv;
            }).SingleInstance();

            builder.Register<IAssetsService>(x =>
            {
                var assetsSrv = new AssetsService(new Uri(_settings.CurrentValue.AssetsServiceClient.ServiceUrl));
                return assetsSrv;
            }).SingleInstance();

            builder.Register<IExchangeOperationsServiceClient>(x =>
            {
                var exchOpSrv = new ExchangeOperationsServiceClient(_settings.CurrentValue.ExchangeOperationsServiceClient.ServiceUrl);
                return exchOpSrv;
            }).SingleInstance();

            builder.RegisterType<BalancesClient>()
                .As<IBalancesClient>()
                .WithParameter("serviceUrl", _settings.CurrentValue.BalancesServiceClient.ServiceUrl)
                .WithParameter("log", _log)
                .SingleInstance();

            builder.RegisterType<FirebaseService>()
                .As<IFirebaseService>()
                .WithParameter("settings", _settings.CurrentValue)
                .SingleInstance();
        }

        private void RegisterDictionaryData(ContainerBuilder builder)
        {
            builder.Register(c =>
            {
                var ctx = c.Resolve<IComponentContext>();
                return new CachedDataDictionary<string, Lykke.Service.Assets.Client.Models.Asset>(
                    async () =>
                        (await ctx.Resolve<IAssetsService>().AssetGetAllWithHttpMessagesAsync()).Body.ToDictionary(itm => itm.Id));   
            }).SingleInstance();

            builder.Register(x =>
            {

                var ctx = x.Resolve<IComponentContext>();

                return new CachedDataDictionary<string, Lykke.Service.Assets.Client.Models.AssetPair>
                (
                    async () => (await ctx.Resolve<IAssetsService>().AssetPairGetAllAsync()).ToDictionary(itm => itm.Id)
                );

            }).SingleInstance();            
        }
    }
}
