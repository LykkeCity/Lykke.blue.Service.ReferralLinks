using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.ReferralLinks.Core.Domain.ReferralLink
{
    public interface IReferralLink
    {
        string Id { get; }
        string Url { get; }
        DateTime UrlExpirationDate { get; }
        string SenderClientId { get; }
        string RecipientClientIdOrEmail { get; }
        string Asset { get; }
        bool? IsNewUser { get; }
        ReferralLinkState State { get; }
        double Amount { get; }
        string ClaimingClientId { get; }
    }
}
