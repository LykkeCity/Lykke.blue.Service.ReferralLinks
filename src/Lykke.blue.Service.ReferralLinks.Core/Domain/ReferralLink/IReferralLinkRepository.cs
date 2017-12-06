using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink
{
    public interface IReferralLinkRepository
    {
        Task<IReferralLink> Create(IReferralLink referralLink);
        Task<IReferralLink> Get(string id);
        Task<IReferralLink> UpdateAsync(IReferralLink referralLink);
        Task<IEnumerable<IReferralLink>> Get(string senderClientId, ReferralLinkState? state);
        Task UpdateState(string id, ReferralLinkState state);
        Task<IEnumerable<IReferralLink>> GetReferralLinksBySenderId(string senderClientId);
        Task SetUrl(string id, string url);
        Task<IEnumerable<IReferralLink>> GetExpiredGiftCoinLinks();
        bool IsInvitationLinkForSenderAlreadyCreated(string senderClientId);
        Task<IReferralLink> GetReferalLinkByUrl(string url);
    }
}
