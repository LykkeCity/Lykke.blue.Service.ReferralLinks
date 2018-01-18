using Lykke.blue.Service.ReferralLinks.Client.AutorestClient.Models;
using System;

namespace Lykke.blue.Service.ReferralLinks.Client.Models
{
    public class ReferralLinkDto
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

        public static ReferralLinkDto Create(GetReferralLinkResponse model)
        {
            return new ReferralLinkDto
            {
                Id = model.Id,
                Url = model.Url,
                ExpirationDate = model.ExpirationDate,
                Asset = model.Asset,
                State = model.State,
                Amount = model.Amount,
                Type = model.Type,
                CreatedAt = model.CreatedAt,
                ClaimsCount = model.ClaimsCount
            };
        }
    }
}
