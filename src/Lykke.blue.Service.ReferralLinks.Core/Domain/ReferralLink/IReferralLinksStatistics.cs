// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global
namespace Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink
{
    public interface IReferralLinksStatistics
    {
        int NumberOfInvitationLinksSent { get; set; }
        int NumberOfInvitationLinksAccepted { get; set; }
        int NumberOfGiftLinksSent { get; set; }
        double AmountOfGiftCoinsDistributed { get; set; }
        int NumberOfNewUsersBroughtIn { get; set; }
    }
}
