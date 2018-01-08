using System;

namespace Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink
{
    public interface IReferralLink
    {
        string Id { get; }
        string Url { get; }
        string SenderClientId { get; }
        DateTime? ExpirationDate { get; }        
        string Asset { get; set; }
        double Amount { get; set; }
        string SenderOffchainTransferId { get; set; }
        string Type { get; }
        string State { get; set; }
        string ETag { get;}
        DateTime? CreatedAt { get; set; }
        int ClaimsCount { get; set; }
    }
}
