using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;

namespace Lykke.blue.Service.ReferralLinks.Services.Domain
{
    public class ReferralLinksStatistics : IReferralLinksStatistics
    {
        public int NumberOfInvitationLinksSent { get; set; }

        public int NumberOfInvitationLinksAccepted { get; set; }

        public int NumberOfGiftLinksSent { get; set; }

        public double AmountOfGiftCoinsDistributed { get; set; }

        public int NumberOfNewUsersBroughtIn { get; set; }
    }
}
