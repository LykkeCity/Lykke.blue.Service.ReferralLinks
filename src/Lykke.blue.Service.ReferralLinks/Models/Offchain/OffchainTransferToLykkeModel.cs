namespace Lykke.blue.Service.ReferralLinks.Models.Offchain
{
    public class TransferToLykkeWallet
    {
        public string ClientId { get; set; }
        public string ReferralLinkId { get; set; }
        public string PrevTempPrivateKey { get; set; }
    }
}
