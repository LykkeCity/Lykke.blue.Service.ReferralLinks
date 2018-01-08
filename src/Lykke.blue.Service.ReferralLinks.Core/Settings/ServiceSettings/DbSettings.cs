// ReSharper disable ClassNeverInstantiated.Global
namespace Lykke.blue.Service.ReferralLinks.Core.Settings.ServiceSettings
{
    public class DbSettings
    {
        public string LogsConnString { get; set; }
        public string ClientPersonalInfoConnString { get; set; }
        public string OffchainConnString { get; set; }
        public string BitCoinQueueConnectionString { get; set; }
        public string ReferralLinksConnString { get; set; }

    }
}
