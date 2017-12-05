namespace Lykke.blue.Service.ReferralLinks.Core.BitCoinApi.Models
{
    public class CloseChannelData
    {
        public string ClientPubKey { get; set; }
        public string AssetId { get; set; }
        public string SignedClosingTransaction { get; set; }
        public string OffchainTransferId { get; set; }
    }
}
