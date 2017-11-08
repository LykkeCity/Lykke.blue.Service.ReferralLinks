using Lykke.Service.ReferralLinks.Core.Domain.ReferralLink;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Lykke.Service.ReferralLinks.Core.Domain.Requests
{
    public class MoneyTransferReferralLinkRequest
    {
        public string SenderClientId { get; set; }
        public string Asset { get; set; }     
        public decimal Amount { get; set; }
       
        [IgnoreDataMember]
        public ReferralLinkType Type => ReferralLinkType.MoneyTransfer;     
    }
}
