using Lykke.Service.Kyc.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Blue.Service.ReferralLinks.Core.Settings.ServiceSettings
{
    public class Services
    {
        public string AssetsServiceUrl { get; set; }
        public string ExchangeOperationsServiceUrl { get; set; }
        public string BitcoinCoreApiUrl { get; set; }
        public KycServiceSettings KycServiceSettings { get; set; }
        public string ClientAccountServiceUrl { get; set; }
        public string BalancesServiceUrl { get; set; }
    }
}
