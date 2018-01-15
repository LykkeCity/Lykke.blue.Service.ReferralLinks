using Lykke.blue.Service.ReferralLinks.Client.AutorestClient.Models;

namespace Lykke.blue.Service.ReferralLinks.Client.Models
{
    public class RequestReferralLinkDto
    {
        public string RefLinkId { get; set; }
        public string RefLinkUrl { get; set; }

        public static RequestReferralLinkDto Create(RequestRefLinkResponse model)
        {
            return new RequestReferralLinkDto
            {
                RefLinkId = model.RefLinkId,
                RefLinkUrl = model.RefLinkUrl
            };
        }
    }
}

