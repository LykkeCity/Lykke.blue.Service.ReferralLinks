using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.ReferralLinks.Core.Domain.ReferralLink
{
    public interface IReferralLinkClaims
    {
        string ReferralLinkId { get; }
        string ClientId { get; }
        bool ShouldReceive { get; }
        bool HasReceived { get; }
        bool IsNewUser { get; }
    }
}
