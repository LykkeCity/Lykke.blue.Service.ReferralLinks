using System;
using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;

namespace Lykke.blue.Service.ReferralLinks.Responses
{
    public class GetReferralLinkResponse : IReferralLink
    {
        public string Id { get; set; }

        public string Url { get; set; }

        public DateTime? ExpirationDate { get; set; }

        public string SenderClientId { get; set; }

        public string Asset { get; set; }        

        public string State { get; set; }

        public double Amount { get; set; }

        public string Type { get; set; }

        public string SenderOffchainTransferId { get; set; }
    }
}
