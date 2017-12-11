namespace Lykke.blue.Service.ReferralLinks.Models.Offchain
{
    public class OffchainFinalizeModel
    {
        public string TransferId { get; set; }
        public string ClientRevokePubKey { get; set; }
        public string ClientRevokeEncryptedPrivateKey { get; set; }
        public string SignedTransferTransaction { get; set; }
        public string ClientId { get; set; }
        public string RefLinkId { get; set; }
    }
}
