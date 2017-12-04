namespace Lykke.blue.Service.ReferralLinks.Models
{
    public class ClaimReferralLinkRequest
    {
        public string RecipientClientId { get; set; }
        public string ReferalLinkId { get; set; }
        public string ReferalLinkUrl { get; set; }
        public bool IsNewClient { get; set; }
    }
}
