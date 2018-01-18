using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using System.Runtime.Serialization;
// ReSharper disable UnusedMember.Global

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
