using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using Lykke.blue.Service.ReferralLinks.Core.Domain.Requests;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
        //Task<bool> IsInvitationLinksMaxNumberReachedForSender(string senderClientId);
        Task CheckForExpiredGiftCoinLink();
        Task<IReferralLink> GetInvitationLinksBySenderId(string senderClientId);
    }
}
