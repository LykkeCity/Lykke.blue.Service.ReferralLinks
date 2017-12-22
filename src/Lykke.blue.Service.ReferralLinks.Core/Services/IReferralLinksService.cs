using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink.Requests;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.Core.Services
{
    public interface IReferralLinksService
    {
        Task<IReferralLink> CreateInvitationLink(InvitationReferralLinkRequest referralLinkRequest);
        Task<IReferralLink> CreateGiftCoinLink(GiftCoinsReferralLinkRequest referralLinkRequest);
        Task<string> CreateGroupOfGiftCoinLinks(GroupGiftCoinLinkRequest referralLinkRequest);
        Task<IReferralLink> Get(string id);
        Task<IEnumerable<IReferralLink>> GetGroup(string groupId);
        Task<IReferralLink> UpdateAsync(IReferralLink referralLink);
        Task<IReferralLink> UpdateAsyncWithETagCheck(IReferralLink referralLink); //must be used for updating ref link tate upon claim
        Task<IReferralLink> GetReferralLinkByUrl(string url);
        Task CheckForExpiredGiftCoinLink();
        IEnumerable<IReferralLink> GetInvitationLinksForSenderId(string senderClientId);
        Task<IEnumerable<IReferralLink>> GetGroupBySenderId(string senderId);
    }
}
