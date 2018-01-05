using System;
// ReSharper disable UnusedMember.Global
// DTO fields, not used internally 

namespace Lykke.blue.Service.ReferralLinks.Responses
{
    public class GetReferralLinkResponse
    {
        public string Id { get; set; }

        public string Url { get; set; }

        public DateTime? ExpirationDate { get; set; }

        public string Asset { get; set; }        

        public string State { get; set; }

        public double Amount { get; set; }

        public string Type { get; set; }

        public DateTime? CreatedAt { get; set; }

        public int ClaimsCount { get; set; }
    }
}
