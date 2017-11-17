using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.blue.Service.ReferralLinks.Models.RefLinkResponseModels
{
    public class ClaimRefLinkResponse
    {
        public string TransactionRewardRecipient { get; set; }
        public string TransactionRewardSender { get; set; }
        public string SenderOffchainTransferId { get; set; }
    }
}
