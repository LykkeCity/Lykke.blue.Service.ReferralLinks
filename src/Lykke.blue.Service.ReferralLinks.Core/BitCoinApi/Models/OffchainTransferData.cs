namespace Lykke.blue.Service.ReferralLinks.Core.BitCoinApi.Models
{
    public class OffchainTransferData
    {
        public string ClientPubKey { get; set; }
        public decimal Amount { get; set; }
        public string AssetId { get; set; }
        public string ClientPrevPrivateKey { get; set; }
        public string ExternalTransferId { get; set; }
        public bool Required { get; set; }
    }
}
