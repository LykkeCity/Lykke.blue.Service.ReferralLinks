using Autofac;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Service.ReferralLinks.AzureRepositories.Kyc;
using Lykke.Service.ReferralLinks.AzureRepositories.ReferralLink;
using Lykke.Service.ReferralLinks.Core.Domain.ReferralLink;
using Lykke.Service.ReferralLinks.Core.Kyc;
using Lykke.Service.ReferralLinks.Core.Settings.ServiceSettings;
using Lykke.SettingsReader;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.ReferralLinks.AzureRepositories
{
    public static class AzureRepoBinder
    {
        public static void BindAzureRepositories(this ContainerBuilder container, IReloadingManager<ReferralLinksSettings> settings, ILog log)
        {
            container.RegisterInstance<ISkipKycRepository>(
               new SkipKycRepository(AzureTableStorage<SkipKycClientEntity>.Create(settings.ConnectionString(n=>n.Services.AssetsServiceUrl), "SkipKycClients", log)));

            container.RegisterInstance<IReferralLinkRepository>(
               new ReferralLinkRepository(AzureTableStorage<ReferralLinkEntity>.Create(settings.ConnectionString(n => n.Db.ReferralLinksConnString), "ReferralLinks", log), settings));
        }
    }
}
