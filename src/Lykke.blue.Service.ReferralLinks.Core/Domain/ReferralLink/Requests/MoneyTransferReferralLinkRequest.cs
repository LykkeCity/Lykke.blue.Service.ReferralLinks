using Lykke.Blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Lykke.Blue.Service.ReferralLinks.Core.Domain.Requests
{
    public class GiftCoinsReferralLinkRequest
    {
        public string SenderClientId { get; set; }
        public string Asset { get; set; }     
        public double Amount { get; set; }
       
        [IgnoreDataMember]
        public ReferralLinkType Type => ReferralLinkType.GiftCoins;     
    }
}
