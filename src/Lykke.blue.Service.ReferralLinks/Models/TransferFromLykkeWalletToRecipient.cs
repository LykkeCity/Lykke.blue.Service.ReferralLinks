using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Blue.Service.ReferralLinks.Models
{
    public class ClaimReferralLinkRequest
    {
        public string RecipientClientId { get; set; }
        public string ReferalLinkId { get; set; }
        public string ReferalLinkUrl { get; set; }
        public bool IsNewClient { get; set; }
    }
}
