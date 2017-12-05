using Lykke.Service.Kyc.Client;

namespace Lykke.blue.Service.ReferralLinks.Core.Settings.ServiceSettings
{
    public class Services
    {
        public string AssetsServiceUrl { get; set; }
        public string ExchangeOperationsServiceUrl { get; set; }
        public string BitcoinCoreApiUrl { get; set; }
        public KycServiceClientSettings KycServiceSettings { get; set; }
        public string ClientAccountServiceUrl { get; set; }
        public string BalancesServiceUrl { get; set; }
    }
}
