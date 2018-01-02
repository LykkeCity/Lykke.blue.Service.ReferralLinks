using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using System.Runtime.Serialization;

namespace Lykke.blue.Service.ReferralLinks.Models.GiftCoinRequests
{
    public class GiftCoinRequestBase
    {
        public string SenderClientId { get; set; }
        public string Asset { get; set; }

        [IgnoreDataMember]
        public ReferralLinkType Type => ReferralLinkType.GiftCoins;
    }
}
