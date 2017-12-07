using Autofac;
using AzureStorage.Blob;
using AzureStorage.Queue;
using AzureStorage.Tables;
using Common.Log;
using Lykke.blue.Service.ReferralLinks.AzureRepositories.Bitcoin;
using Lykke.blue.Service.ReferralLinks.AzureRepositories.Client;
using Lykke.blue.Service.ReferralLinks.AzureRepositories.Offchain;
using Lykke.blue.Service.ReferralLinks.AzureRepositories.ReferralLink;
using Lykke.blue.Service.ReferralLinks.AzureRepositories.WalletCredentials;
using Lykke.blue.Service.ReferralLinks.Core.BitCoinApi;
using Lykke.blue.Service.ReferralLinks.Core.Domain.Client;
using Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain;
using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using Lykke.blue.Service.ReferralLinks.Core.Domain.WalletCredentials;
using Lykke.blue.Service.ReferralLinks.Core.Settings;
using Lykke.SettingsReader;

namespace Lykke.blue.Service.ReferralLinks.AzureRepositories
{
    public static class AzureRepoBinder
    {
        public static void BindAzureRepositories(this ContainerBuilder container, IReloadingManager<AppSettings> settings, ILog log)
        {
            container.RegisterInstance<IWalletCredentialsRepository>(
               new WalletCredentialsRepository(AzureTableStorage<WalletCredentialsEntity>.Create(settings.ConnectionString(n => n.ReferralLinksService.Db.ClientPersonalInfoConnString), "WalletCredentials", log)));

            container.RegisterInstance<IOffchainTransferRepository>(
               new OffchainTransferRepository(AzureTableStorage<OffchainTransferEntity>.Create(settings.ConnectionString(n => n.ReferralLinksService.Db.OffchainConnString), "OffchainTransfers", log)));

            container.RegisterInstance<IClientSettingsRepository>(
               new ClientSettingsRepository(AzureTableStorage<ClientSettingsEntity>.Create(settings.ConnectionString(n => n.ReferralLinksService.Db.ClientPersonalInfoConnString), "TraderSettings", log)));

            container.RegisterInstance<IOffchainEncryptedKeysRepository>(
               new OffchainEncryptedKeyRepository(
                   AzureTableStorage<OffchainEncryptedKeyEntity>.Create(settings.ConnectionString(x => x.ReferralLinksService.Db.OffchainConnString), "OffchainEncryptedKeys", log)));

            container.RegisterInstance<IOffchainRequestRepository>(
               new OffchainRequestRepository(
                   AzureTableStorage<OffchainRequestEntity>.Create(settings.ConnectionString(x => x.ReferralLinksService.Db.OffchainConnString), "OffchainRequests", log)));
            
           container.RegisterInstance(new BitcoinTransactionContextBlobStorage(AzureBlobStorage.Create(settings.ConnectionString(x => x.ReferralLinksService.Db.BitCoinQueueConnectionString))))
                .As<IBitcoinTransactionContextBlobStorage>();

            container.RegisterInstance<IReferralLinkRepository>(
               new ReferralLinkRepository(AzureTableStorage<ReferralLinkEntity>.Create(settings.ConnectionString(n => n.ReferralLinksService.Db.ReferralLinksConnString), "ReferralLinks", log)));

            container.RegisterInstance<IReferralLinkClaimsRepository>(
               new ReferralLinkClaimsRepository(AzureTableStorage<ReferralLinkClaimEntity>.Create(settings.ConnectionString(n => n.ReferralLinksService.Db.ReferralLinksConnString), "ReferralLinkClaims", log)));

            container.RegisterInstance<IOffchainFinalizeCommandProducer>(new OffchainFinalizeCommandProducer(AzureQueueExt.Create(settings.ConnectionString(x => x.ReferralLinksService.Db.BitCoinQueueConnectionString), "offchain-finalization")));
        }
    }
}
