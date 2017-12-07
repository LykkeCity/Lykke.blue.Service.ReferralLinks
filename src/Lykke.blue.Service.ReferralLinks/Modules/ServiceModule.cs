﻿using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common;
using Common.Log;
using Lykke.blue.Service.ReferralLinks.AzureRepositories;
using Lykke.blue.Service.ReferralLinks.Core.BitCoinApi;
using Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain;
using Lykke.blue.Service.ReferralLinks.Core.Kyc;
using Lykke.blue.Service.ReferralLinks.Core.Services;
using Lykke.blue.Service.ReferralLinks.Core.Settings;
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
using System;
using System.Linq;
using System.Net;

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
        }

        private static IPEndPoint GetIpEndPointFromHostName(string hostName, int port)
        {
            var addresses = Dns.GetHostAddresses(hostName);
            if (addresses.Length == 0)
            {
                throw new ArgumentException($"Unable to retrieve address from specified host name: {hostName}", nameof(hostName));
            }
            if (addresses.Length > 1)
            {
                throw new ArgumentException("There is more that one IP address to the specified host.", nameof(hostName));
            }
            return new IPEndPoint(addresses[0], port);     
        }

        private void RegisterLocalServices(ContainerBuilder builder)
        {
            builder.RegisterType<HealthService>().As<IHealthService>().SingleInstance();
            builder.RegisterType<BitcoinTransactionService>().As<IBitcoinTransactionService>().SingleInstance();
            builder.RegisterType<SrvKycForAsset>().As<ISrvKycForAsset>().SingleInstance();
            
            builder.RegisterType<OffchainRequestService>().As<IOffchainRequestService>();
            builder.RegisterType<OffchainService>().As<IOffchainService>().SingleInstance();

            builder.BindMeClient(GetIpEndPointFromHostName(
                _settings.CurrentValue.MatchingEngineClient.IpEndpoint.Host,
                _settings.CurrentValue.MatchingEngineClient.IpEndpoint.Port));

            builder.RegisterType<ReferralLinksService>().As<IReferralLinksService>().SingleInstance();
            builder.RegisterType<ReferralLinkClaimsService>().As<IReferralLinkClaimsService>().SingleInstance();
            builder.RegisterType<StatisticsService>().As<IStatisticsService>().SingleInstance();

            builder.RegisterType<ExchangeService>().SingleInstance();
            builder.RegisterType<BitcoinApiClientLocal>().SingleInstance();
        }

        private void RegisterRepos(ContainerBuilder builder)
        {
            builder.BindAzureRepositories(_settings, _log);
        }

        private void RegisterExternalServices(ContainerBuilder builder)
        {
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

            builder.RegisterType<Lykke.Service.ClientAccount.Client.ClientAccountClient>()
                .As<IClientAccountClient>()
                .WithParameter("serviceUrl", _settings.CurrentValue.ClientAccountClient.ServiceUrl)
                .WithParameter("log", _log)
                .SingleInstance();

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
