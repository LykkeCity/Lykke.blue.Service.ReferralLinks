using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.ReferralLinks.Core.Domain.ReferralLink
{
    public interface IReferralLinkRepository
    {
        Task<IReferralLink> Create(IReferralLink referralLink);
        Task<IReferralLink> Get(string id);
        Task<IReferralLink> Update(IReferralLink referralLink);
        Task Delete(string id);
        Task<IEnumerable<IReferralLink>> Get(string senderClientId, string state);
        Task UpdateState(string id, string state);
        Task<IReferralLinksStatistics> GetReferralLinksStatisticsBySenderId(string senderClientId);
        Task SetUrl(string id, string url);
        Task<string> ClaimGiftCoins(string id, bool isNewUser, string claimingUserId);
        Task ReturnCoinsToSender();
    }
}
