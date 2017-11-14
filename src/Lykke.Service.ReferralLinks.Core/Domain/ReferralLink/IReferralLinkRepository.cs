using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Blue.Service.ReferralLinks.Core.Domain.ReferralLink
{
    public interface IReferralLinkRepository
    {
        Task<IReferralLink> Create(IReferralLink referralLink);
        Task<IReferralLink> Get(string id);
        Task<IReferralLink> UpdateAsync(IReferralLink referralLink);
        Task Delete(string id);
        Task<IEnumerable<IReferralLink>> Get(string senderClientId, ReferralLinkState? state);
        Task UpdateState(string id, ReferralLinkState state);
        Task<IEnumerable<IReferralLink>> GetReferralLinksBySenderId(string senderClientId);
        Task SetUrl(string id, string url);
        Task ReturnCoinsToSender();
        Task<bool> IsInvitationLinksMaxNumberReachedForSender(string claimingClientId);
        Task<IReferralLink> GetReferalLinkByUrl(string url);
    }
}
