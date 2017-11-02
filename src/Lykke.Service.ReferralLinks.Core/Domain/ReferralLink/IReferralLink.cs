using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.ReferralLinks.Core.Domain.ReferralLink
{
    public interface IReferralLink
    {
        string Id { get; }
        string Url { get; }
        string SenderClientId { get; }
        DateTime? ExpirationDate { get; }        
        string Asset { get; }
        double Amount { get; }
        string SenderTransactionId { get; }
        ReferralLinkType Type { get; }
        ReferralLinkState State { get; }

        bool? IsNewUser { get; }
        string ClaimingClientId { get; }
    }
}
