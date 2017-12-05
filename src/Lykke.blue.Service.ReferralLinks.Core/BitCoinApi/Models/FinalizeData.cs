namespace Lykke.blue.Service.ReferralLinks.Core.BitCoinApi.Models
{
    public class FinalizeData
    {
        public string ClientPubKey { get; set; }
        public string AssetId { get; set; }
        public string ClientRevokePubKey { get; set; }
        public string SignedByClientHubCommitment { get; set; }
        public string ExternalTransferId { get; set; }
        public string OffchainTransferId { get; set; }
    }
}
