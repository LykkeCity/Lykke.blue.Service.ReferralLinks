using System.Runtime.Serialization;
using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;

namespace Lykke.blue.Service.ReferralLinks.Core.Domain.Requests
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
