using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.ReferralLinks.Models
{
    public class TransferFromLykkeWalletToRecipient
    {
        public string RecipientClientId { get; set; }
        public string ReferalLinkId { get; set; }
        public bool IsNewClient { get; set; }
    }
}
