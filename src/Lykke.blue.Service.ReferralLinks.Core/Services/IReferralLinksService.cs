using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using Lykke.blue.Service.ReferralLinks.Core.Domain.Requests;

namespace Lykke.blue.Service.ReferralLinks.Core.Services
{
    public interface IReferralLinksService
    {
        Task<IReferralLink> CreateInvitationLink(InvitationReferralLinkRequest referralLinkRequest);
        Task<IReferralLink> CreateGiftCoinsLink(GiftCoinsReferralLinkRequest referralLinkRequest);
        Task<IReferralLink> Get(string id);
        Task<IReferralLink> UpdateAsync(IReferralLink referralLink);
        Task UpdateState(string id, ReferralLinkState state);        
        Task SetUrl(string id, string url);
        Task<IReferralLink> GetReferralLinkById(string id);
        Task<IReferralLink> GetReferralLinkByUrl(string url);
        bool InvitationLinkForSenderIdExists(string senderClientId);
        Task CheckForExpiredGiftCoinLink();
        IEnumerable<IReferralLink> GetInvitationLinksForSenderId(string senderClientId);
    }
}
