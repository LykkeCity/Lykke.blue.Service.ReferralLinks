using System.Runtime.Serialization;
// ReSharper disable UnusedMember.Global

namespace Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink.Requests
{
    public class InvitationReferralLinkRequest 
    {
        public string SenderClientId { get; set; }      

        [IgnoreDataMember]
        public ReferralLinkType Type => ReferralLinkType.Invitation;

    }
}
