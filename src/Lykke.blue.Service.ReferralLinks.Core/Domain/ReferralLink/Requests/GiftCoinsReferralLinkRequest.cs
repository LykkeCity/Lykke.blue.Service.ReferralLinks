using System.Runtime.Serialization;

namespace Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink.Requests
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
