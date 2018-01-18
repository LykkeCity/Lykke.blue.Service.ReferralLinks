using Lykke.blue.Service.ReferralLinks.Client.AutorestClient.Models;

namespace Lykke.blue.Service.ReferralLinks.Client.Models
{
    public class ReferralLinksStatisticsDto
    {
        public int NumberOfInvitationLinksSent { get; set; }

        public int NumberOfInvitationLinksAccepted { get; set; }

        public int NumberOfGiftLinksSent { get; set; }

        public double AmountOfGiftCoinsDistributed { get; set; }

        public int NumberOfNewUsersBroughtIn { get; set; }

        public static ReferralLinksStatisticsDto Create(GetReferralLinksStatisticsBySenderIdResponse model)
        {
            return new ReferralLinksStatisticsDto
            {
                NumberOfInvitationLinksSent = model.NumberOfInvitationLinksSent,
                NumberOfInvitationLinksAccepted = model.NumberOfInvitationLinksAccepted,
                NumberOfGiftLinksSent = model.NumberOfGiftLinksSent,
                AmountOfGiftCoinsDistributed = model.AmountOfGiftCoinsDistributed,
                NumberOfNewUsersBroughtIn = model.NumberOfNewUsersBroughtIn
            };
        }
    }
}
