using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.ReferralLinks.Models.Offchain
{
    public class OffchainTransferToLykkeModel
    {
        public string ClientId { get; set; }
        public string WalletId { get; set; }
        public string Asset { get; set; }
        public decimal Amount { get; set; }
        public string PrevTempPrivateKey { get; set; }
    }
}
