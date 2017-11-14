using Lykke.Blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Blue.Service.ReferralLinks.Services.Domain
{
    public class ReferralLinkClaim : IReferralLinkClaim
    {
        public string Id { get; set; }

        public string ReferralLinkId { get; set; }

        public string RecipientClientId { get; set; }

        public string RecipientTransactionId { get; set; }

        public bool ShouldReceiveReward { get; set; }

        public bool IsNewClient { get; set; }
}
}
