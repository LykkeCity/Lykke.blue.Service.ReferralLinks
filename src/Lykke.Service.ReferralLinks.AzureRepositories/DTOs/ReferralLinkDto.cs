using System;
using Lykke.Service.ReferralLinks.Core.Domain.ReferralLink;

namespace Lykke.Service.ReferralLinks.AzureRepositories.DTOs
{
    public class ReferralLinkDto : IReferralLink
    {
        public string Id { get; set; }

        public string Url { get; set; }

        public DateTime UrlExpirationDate { get; set; }

        public string ClientIdSender { get; set; }

        public string RecipientEmail { get; set; }

        public string AssetId { get; set; }

        public RecipientType RecipientType { get; set; }

        public ReferralLinkState State { get; set; }
    }
}
