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
        string Asset { get; set; }
        decimal Amount { get; set; }
        string SenderTransactionId { get; set; }
        ReferralLinkType Type { get; }
        ReferralLinkState State { get; set; }
    }
}
