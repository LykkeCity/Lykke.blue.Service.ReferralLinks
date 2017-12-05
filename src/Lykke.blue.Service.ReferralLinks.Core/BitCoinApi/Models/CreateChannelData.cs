namespace Lykke.blue.Service.ReferralLinks.Core.BitCoinApi.Models
{
    public class CreateChannelData
    {
        public string ClientPubKey { get; set; }
        public decimal ClientAmount { get; set; }
        public decimal HubAmount { get; set; }
        public string AssetId { get; set; }
        public string ExternalTransferId { get; set; }
        public bool Required { get; set; }
    }
}
