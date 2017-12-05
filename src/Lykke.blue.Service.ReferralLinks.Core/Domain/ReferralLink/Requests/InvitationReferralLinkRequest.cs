using System.Runtime.Serialization;
using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;

namespace Lykke.blue.Service.ReferralLinks.Core.Domain.Requests
{
    public class InvitationReferralLinkRequest 
    {
        public string SenderClientId { get; set; }      

        [IgnoreDataMember]
        public ReferralLinkType Type => ReferralLinkType.Invitation;

    }
}
