using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using System;

namespace Lykke.blue.Service.ReferralLinks.Services.Domain
{
    public class ReferralLink : IReferralLink
    {
        public string Id { get; private set; }

        public string Url { get; set; }

        public string SenderClientId { get; private set; }

        public DateTime? ExpirationDate { get; private set; }

        public string Asset { get; set; }

        public double Amount { get; set; }

        public string SenderOffchainTransferId { get; set; }

        public string Type { get; private set; }

        public string State { get; set; }

        public string ETag { get; set; }

        public DateTime? CreatedAt { get ; set ; }

        public int ClaimsCount { get; set; }

        public static ReferralLink Create(string senderId, string asset, double amount, ReferralLinkType type, string url = null, DateTime? expirationDate = null )
        {
            return new ReferralLink
            {
                Id = Guid.NewGuid().ToString(),
                Url = url,
                SenderClientId = senderId,
                ExpirationDate = expirationDate,
                Asset = asset,
                Amount = amount,
                Type = type.ToString(),
                State = ReferralLinkState.Created.ToString(),
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
