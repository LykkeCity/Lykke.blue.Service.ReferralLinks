using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.ReferralLinks.Models.Offchain
{
    public class OffchainFinalizeModel
    {
        public string TransferId { get; set; }
        public string ClientRevokePubKey { get; set; }
        public string ClientRevokeEncryptedPrivateKey { get; set; }
        public string SignedTransferTransaction { get; set; }
        public string ClientId { get; set; }
    }
}
