using Lykke.blue.Service.ReferralLinks.Core.Settings.ServiceSettings;
using Lykke.Service.Kyc.Client;
using System.Net;

namespace Lykke.blue.Service.ReferralLinks.Core.Settings
{
    public class AppSettings
    {
        public ReferralLinksSettings ReferralLinksService { get; set; }
        public MatchingEngineSettings MatchingEngineClient { get; set; }
        public AssetsServiceClient AssetsServiceClient { get; set; }
        public ClientAccountClient ClientAccountClient { get; set; }
        public BitCoinCoreApiClient BitCoinCore { get; set; }
        public BalancesServiceClient BalancesServiceClient { get; set; }
        public KycServiceClientSettings KycServiceClient { get; set; }
        public ExchangeOperationsClient ExchangeOperationsServiceClient { get; set; }

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

    public class ClientAccountClient
    {
        public string ServiceUrl { get; set; }
    }

    public class MatchingEngineSettings
    {
        public IpEndpointSettings IpEndpoint { get; set; }
    }

    public class IpEndpointSettings
    {
        public string InternalHost { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }

        public IPEndPoint GetClientIpEndPoint(bool useInternal = false)
        {
            string host = useInternal ? InternalHost : Host;

            if (IPAddress.TryParse(host, out var ipAddress))
                return new IPEndPoint(ipAddress, Port);

            var addresses = Dns.GetHostAddressesAsync(host).Result;
            return new IPEndPoint(addresses[0], Port);
        }
    }

    
}
