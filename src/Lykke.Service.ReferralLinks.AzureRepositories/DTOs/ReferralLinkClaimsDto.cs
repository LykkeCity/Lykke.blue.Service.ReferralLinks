using Lykke.Service.ReferralLinks.Core.Domain.ReferralLink;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.ReferralLinks.AzureRepositories.DTOs
{
    public class ReferralLinkClaimsDto : IReferralLinkClaims
    {
        public string ReferralLinkId { get; set; }

        public string ClientId { get; set; }

        public bool ShouldReceive { get; set; }

        public bool HasReceived { get; set; }

        public bool IsNewUser { get; set; }
    }
}
