using Lykke.Service.ReferralLinks.Core.Domain.ReferralLink;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.ReferralLinks.Core.Services
{
    public interface IReferralLinksService
    {
        Task<IReferralLink> Create(IReferralLink referralLink);
        Task<IReferralLink> Get(string id);
        Task<IEnumerable<IReferralLink>> GetReferralLinksBySenderClientIdAndOrStatus(string clientId, string state);
        Task UpdateState(string id, string state);
        Task<IReferralLinksStatistics> GetReferralLinksStatisticsBySenderId(string senderClientId);
        Task SetUrl(string id, string url);
        Task<string> ClaimGiftCoins(string id, bool isNewUser, string claimingClientId);
        Task ReturnCoinsToSender();
        Task<bool> IsReferralLinksNumberLimitReached(string senderClientId);
    }
}
