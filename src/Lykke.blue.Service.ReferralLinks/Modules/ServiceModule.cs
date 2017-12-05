using System;
using System.Linq;
using System.Net;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common;
using Common.Log;
using Lykke.blue.Service.ReferralLinks.AzureRepositories;
using Lykke.blue.Service.ReferralLinks.Core.BitCoinApi;
using Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain;
using Lykke.blue.Service.ReferralLinks.Core.Kyc;
using Lykke.blue.Service.ReferralLinks.Core.Services;
using Lykke.blue.Service.ReferralLinks.Core.Settings.ServiceSettings;
using Lykke.blue.Service.ReferralLinks.Services;
using Lykke.blue.Service.ReferralLinks.Services.Bitcoin;
using Lykke.blue.Service.ReferralLinks.Services.ExchangeOperations;
using Lykke.blue.Service.ReferralLinks.Services.Kyc;
using Lykke.blue.Service.ReferralLinks.Services.Offchain;
using Lykke.Service.Assets.Client;
using Lykke.Service.Balances.Client;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ExchangeOperations.Client;
using Lykke.Service.Kyc.Abstractions.Services;
using Lykke.Service.Kyc.Client;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.blue.Service.ReferralLinks.Modules
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
            
        }

        public static IPEndPoint GetIPEndPointFromHostName(string hostName, int port, bool throwIfMoreThanOneIP)
        {
            var addresses = System.Net.Dns.GetHostAddresses(hostName);
            if (addresses.Length == 0)
            {
                throw new ArgumentException(
                    "Unable to retrieve address from specified host name.",
                    "hostName"
                );
            }
            else if (throwIfMoreThanOneIP && addresses.Length > 1)
            {
                throw new ArgumentException(
                    "There is more that one IP address to the specified host.",
                    "hostName"
                );
            }
            return new IPEndPoint(addresses[0], port); // Port gets validated here.
        }

        private void RegisterLocalServices(ContainerBuilder builder)
        {
            builder.RegisterType<HealthService>().As<IHealthService>().SingleInstance();
            builder.RegisterType<BitcoinTransactionService>().As<IBitcoinTransactionService>().SingleInstance();
            builder.RegisterInstance(_settings.CurrentValue.ExternalServices.KycServiceSettings);
            builder.RegisterType<KycStatusServiceClient>().As<IKycStatusService>().SingleInstance();
            builder.RegisterType<SrvKycForAsset>().As<ISrvKycForAsset>().SingleInstance();
            builder.RegisterType<BitcoinApiClient>().As<IBitcoinApiClient>().SingleInstance();
            builder.RegisterType<OffchainRequestService>().As<IOffchainRequestService>();
            builder.RegisterType<OffchainService>().As<IOffchainService>().SingleInstance();

            builder.BindMeClient(GetIPEndPointFromHostName("me.lykke-me.svc.cluster.local", 8888, true));

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
            builder.Register<IAssetsService>(x =>
            {
                var assetsSrv = new AssetsService(new Uri(_settings.CurrentValue.ExternalServices.AssetsServiceUrl));
                return assetsSrv;
            }).SingleInstance();

            builder.Register<IExchangeOperationsServiceClient>(x =>
            {
                var exchOpSrv = new ExchangeOperationsServiceClient(_settings.CurrentValue.ExternalServices.ExchangeOperationsServiceUrl);
                return exchOpSrv;
            }).SingleInstance();

            builder.RegisterType<ClientAccountClient>()
                .As<IClientAccountClient>()
                .WithParameter("serviceUrl", _settings.CurrentValue.ExternalServices.ClientAccountServiceUrl)
                .WithParameter("log", _log)
                .SingleInstance();

            builder.RegisterType<BalancesClient>()
                .As<IBalancesClient>()
                .WithParameter("serviceUrl", _settings.CurrentValue.ExternalServices.BalancesServiceUrl)
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
                        (await ctx.Resolve<Lykke.Service.Assets.Client.IAssetsService>().AssetGetAllWithHttpMessagesAsync()).Body.ToDictionary(itm => itm.Id)); //.Select(a => a.ConvertToServiceModel())
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
