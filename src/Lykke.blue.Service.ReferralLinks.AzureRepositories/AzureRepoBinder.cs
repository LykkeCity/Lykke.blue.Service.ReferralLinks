using Autofac;
using AzureStorage.Tables;
using Common.Log;
using Lykke.blue.Service.ReferralLinks.AzureRepositories.ReferralLink;
using Lykke.blue.Service.ReferralLinks.AzureRepositories.WalletCredentials;
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

            container.RegisterInstance<IReferralLinkRepository>(
               new ReferralLinkRepository(AzureTableStorage<ReferralLinkEntity>.Create(settings.ConnectionString(n => n.ReferralLinksService.Db.ReferralLinksConnString), "ReferralLinks", log)));

            container.RegisterInstance<IReferralLinkClaimsRepository>(
               new ReferralLinkClaimsRepository(AzureTableStorage<ReferralLinkClaimEntity>.Create(settings.ConnectionString(n => n.ReferralLinksService.Db.ReferralLinksConnString), "ReferralLinkClaims", log)));          
        }
    }
}
