using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using System.Runtime.Serialization;

namespace Lykke.blue.Service.ReferralLinks.Core.Domain.Requests
{
    public class InvitationReferralLinkRequest 
    {
        public string SenderClientId { get; set; }      

        [IgnoreDataMember]
        public ReferralLinkType Type => ReferralLinkType.Invitation;

    }
}
