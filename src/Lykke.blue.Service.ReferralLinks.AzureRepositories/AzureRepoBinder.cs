using Autofac;
using AzureStorage.Blob;
using AzureStorage.Queue;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Logs;
using Lykke.Blue.Service.ReferralLinks.AzureRepositories.Bitcoin;
using Lykke.Blue.Service.ReferralLinks.AzureRepositories.Client;
using Lykke.Blue.Service.ReferralLinks.AzureRepositories.Kyc;
using Lykke.Blue.Service.ReferralLinks.AzureRepositories.ReferralLink;
using Lykke.Blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using Lykke.Blue.Service.ReferralLinks.AzureRepositories.Offchain;
using Lykke.Blue.Service.ReferralLinks.AzureRepositories.WalletCredentials;
using Lykke.Blue.Service.ReferralLinks.Core.BitCoinApi;
using Lykke.Blue.Service.ReferralLinks.Core.Domain.Client;
using Lykke.Blue.Service.ReferralLinks.Core.Domain.Offchain;
using Lykke.Blue.Service.ReferralLinks.Core.Kyc;
using Lykke.Blue.Service.ReferralLinks.Core.Settings.ServiceSettings;
using Lykke.SettingsReader;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Blue.Service.ReferralLinks.AzureRepositories
{
    public static class AzureRepoBinder
    {
        public static void BindAzureRepositories(this ContainerBuilder container, IReloadingManager<ReferralLinksSettings> settings, ILog log)
        {
            container.RegisterInstance<ISkipKycRepository>(
               new SkipKycRepository(AzureTableStorage<SkipKycClientEntity>.Create(settings.ConnectionString(n=> n.Db.ClientPersonalInfoConnString), "SkipKycClients", log)));

            container.RegisterInstance<IWalletCredentialsRepository>(
               new WalletCredentialsRepository(AzureTableStorage<WalletCredentialsEntity>.Create(settings.ConnectionString(n => n.Db.ClientPersonalInfoConnString), "WalletCredentials", log)));

            container.RegisterInstance<IOffchainTransferRepository>(
               new OffchainTransferRepository(AzureTableStorage<OffchainTransferEntity>.Create(settings.ConnectionString(n => n.Db.OffchainConnString), "OffchainTransfers", log)));

            container.RegisterInstance<IClientSettingsRepository>(
               new ClientSettingsRepository(AzureTableStorage<ClientSettingsEntity>.Create(settings.ConnectionString(n => n.Db.ClientPersonalInfoConnString), "TraderSettings", log)));

            container.RegisterInstance<IOffchainEncryptedKeysRepository>(
               new OffchainEncryptedKeyRepository(
                   AzureTableStorage<OffchainEncryptedKeyEntity>.Create(settings.ConnectionString(x => x.Db.OffchainConnString), "OffchainEncryptedKeys", log)));

            container.RegisterInstance<IOffchainOrdersRepository>(
                new OffchainOrderRepository(
                    AzureTableStorage<OffchainOrder>.Create(settings.ConnectionString(x => x.Db.OffchainConnString), "OffchainOrders", log)));

            container.RegisterInstance<IOffchainRequestRepository>(
               new OffchainRequestRepository(
                   AzureTableStorage<OffchainRequestEntity>.Create(settings.ConnectionString(x => x.Db.OffchainConnString), "OffchainRequests", log)));
            
            container.RegisterInstance<IBitCoinTransactionsRepository>(
              new BitCoinTransactionsRepository(
                  AzureTableStorage<BitCoinTransactionEntity>.Create(settings.ConnectionString(x => x.Db.BitCoinQueueConnectionString),
                      "BitCoinTransactions", log)));

            container.RegisterInstance(new BitcoinTransactionContextBlobStorage(AzureBlobStorage.Create(settings.ConnectionString(x => x.Db.BitCoinQueueConnectionString))))
                .As<IBitcoinTransactionContextBlobStorage>();

            container.RegisterInstance<IReferralLinkRepository>(
               new ReferralLinkRepository(AzureTableStorage<ReferralLinkEntity>.Create(settings.ConnectionString(n => n.Db.ReferralLinksConnString), "ReferralLinks", log), settings.CurrentValue));

            container.RegisterInstance<IReferralLinkClaimsRepository>(
               new ReferralLinkClaimsRepository(AzureTableStorage<ReferralLinkClaimEntity>.Create(settings.ConnectionString(n => n.Db.ReferralLinksConnString), "ReferralLinkClaims", log)));

            container.RegisterInstance<IOffchainFinalizeCommandProducer>(new OffchainFinalizeCommandProducer(AzureQueueExt.Create(settings.ConnectionString(x => x.Db.BitCoinQueueConnectionString), "offchain-finalization")));
        }

        public static ILog BindLog(this ContainerBuilder container, IReloadingManager<string> connectionString, string appName, string tableName)
        {
            var consoleLogger = new LogToConsole();

            var persistenceManager = new LykkeLogToAzureStoragePersistenceManager(
                appName,
                AzureTableStorage<LogEntity>.Create(connectionString, tableName, consoleLogger),
                consoleLogger);

            var azureStorageLogger = new LykkeLogToAzureStorage(
                appName,
                persistenceManager,
                lastResortLog: consoleLogger,
                ownPersistenceManager: true);

            azureStorageLogger.Start();

            container.RegisterInstance<ILog>(azureStorageLogger);

            return azureStorageLogger;
        }
    }
}
