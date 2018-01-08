using Lykke.blue.Service.ReferralLinks.Core.Settings.ServiceSettings;
using Lykke.blue.Service.ReferralLinks.Core.Settings.SlackNotifications;
using Lykke.Service.Kyc.Client;
// ReSharper disable ClassNeverInstantiated.Global

namespace Lykke.blue.Service.ReferralLinks.Core.Settings
{
    public class AppSettings
    {
        public ReferralLinksSettings ReferralLinksService { get; set; }
        public AssetsServiceClient AssetsServiceClient { get; set; }
        public BitCoinCoreApiClient BitCoinCore { get; set; }
        public BalancesServiceClient BalancesServiceClient { get; set; }
        public KycServiceClientSettings KycServiceClient { get; set; }
        public ExchangeOperationsClient ExchangeOperationsServiceClient { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
    }

    public class ExchangeOperationsClient
    {
        public string ServiceUrl { get; set; }
    }

    public class AssetsServiceClient
    {
        public string ServiceUrl { get; set; }
    }

    public class BalancesServiceClient
    {
        public string ServiceUrl { get; set; }
    }

    public class BitCoinCoreApiClient
    {
        public string BitcoinCoreApiUrl { get; set; }
    }
}
