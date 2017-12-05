namespace Lykke.blue.Service.ReferralLinks.Core.BitCoinApi.Models
{
    public class OffchainChannelBalance
    {
        public string Multisig { get; set; }
        public decimal ClientAmount { get; set; }
        public decimal HubAmount { get; set; }
        public string Hash { get; set; }
        public bool Actual { get; set; }
    }
}
