using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.ReferralLinks.Models.Offchain
{
    public class TransferToLykkeWallet
    {
        public string ClientId { get; set; }
        public string ReferralLinkId { get; set; }
        public string PrevTempPrivateKey { get; set; }
    }
}
