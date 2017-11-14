using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Blue.Service.ReferralLinks.Core.Domain.ReferralLink
{
    public interface IReferralLinkClaim
    {
        string Id { get; }
        string ReferralLinkId { get; }
        string RecipientClientId { get; }
        string RecipientTransactionId { get; set; }
        bool ShouldReceiveReward { get; }
        bool IsNewClient { get; }
    }
}
