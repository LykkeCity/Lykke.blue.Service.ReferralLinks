namespace Lykke.blue.Service.ReferralLinks.Core.BitCoinApi.Models
{
    public class CreateHubComitmentData
    {
        public string ClientPubKey { get; set; }
        public decimal Amount { get; set; }
        public string AssetId { get; set; }
        public string SignedByClientChannel { get; set; }
    }
}
