using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink.Requests;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.Core.Services
{
    public interface IReferralLinksService
    {
        Task<IReferralLink> CreateInvitationLink(InvitationReferralLinkRequest referralLinkRequest);
        Task<IReferralLink> CreateGiftCoinLink(string senderId, string assetId, ReferralLinkType type, double amount);
        Task<string> CreateGroupOfGiftCoinLinks(string senderId, string assetId, ReferralLinkType type, double[] linksAmounts);
        Task<IReferralLink> Get(string id);
        Task<bool> HasEnoughBalance(string clientId, string assetId, double amount);
        Task<IEnumerable<IReferralLink>> GetGroup(string groupId);
        Task<IReferralLink> UpdateAsync(IReferralLink referralLink);
        Task<IReferralLink> UpdateAsyncWithETagCheck(IReferralLink referralLink); //must be used for updating ref link tate upon claim
        Task<IReferralLink> GetReferralLinkByUrl(string url);
        Task CheckForExpiredGiftCoinLink();
        IEnumerable<IReferralLink> GetInvitationLinksForSenderId(string senderClientId);
        Task<IEnumerable<IReferralLink>> GetGroupBySenderId(string senderId);
    }
}
