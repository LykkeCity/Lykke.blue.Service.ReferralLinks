using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink
{
    public interface IReferralLinkRepository
    {
        Task<IReferralLink> Create(IReferralLink referralLink);
        Task<IReferralLink> Get(string id);
        Task<IReferralLink> UpdateAsync(IReferralLink referralLink);
        Task<IEnumerable<IReferralLink>> GetReferralLinksBySenderId(string senderClientId);
        Task<IEnumerable<IReferralLink>> GetExpiredGiftCoinLinks();
        bool IsInvitationLinkForSenderAlreadyCreated(string senderClientId);
        Task<IReferralLink> GetReferalLinkByUrl(string url);
    }
}
