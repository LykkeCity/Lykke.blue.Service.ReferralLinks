using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink.Requests;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.Core.Services
{
    public interface IReferralLinksService
    {
        Task<IReferralLink> CreateInvitationLink(InvitationReferralLinkRequest req);
        Task<IReferralLink> CreateGiftCoinLink(string senderId, string assetId, double amount);
        Task<List<IReferralLink>> CreateGroupOfGiftCoinLinks(string senderId, string assetId, double[] linksAmounts);
        Task<IReferralLink> Get(string id);
        Task<bool> HasEnoughBalance(string clientId, string assetId, double amount);
        Task<IReferralLink> UpdateAsync(IReferralLink referralLink);
        Task<IReferralLink> UpdateAsyncWithETagCheck(IReferralLink referralLink); 
        Task<IReferralLink> GetReferralLinkByUrl(string url);
        Task CheckForExpiredGiftCoinLink();
        IEnumerable<IReferralLink> GetInvitationLinksForSenderId(string senderClientId);
        Task<IEnumerable<IReferralLink>> GetGiftCoinLinksBySenderId(string senderClientId);
    }
}
