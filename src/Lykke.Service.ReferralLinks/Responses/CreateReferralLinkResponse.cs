using System;
using Lykke.Service.ReferralLinks.Core.Domain.ReferralLink;

namespace Lykke.Service.ReferralLinks.Responses
{
    public class CreateReferralLinkResponse : IReferralLink
    {
        public string Id { get; set; }

        public string Url { get; set; }

        public DateTime? ExpirationDate { get; set; }

        public string SenderClientId { get; set; }

        public string Asset { get; set; }

        public bool? IsNewUser { get; set; }

        public ReferralLinkState State { get; set; }

        public double? Amount { get; set; }

        public string ClaimingClientId { get; set; }

        public ReferralLinkType Type { get; set; }

        public string SenderTransactionId { get; set; }
    }
}
