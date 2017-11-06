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
        Task<IReferralLink> UpdateAsync(IReferralLink referralLink);
        Task<IEnumerable<IReferralLink>> GetReferralLinksBySenderClientIdAndOrStatus(string clientId, ReferralLinkState state);
        Task UpdateState(string id, ReferralLinkState state);
        Task<IReferralLinksStatistics> GetReferralLinksStatisticsBySenderId(string senderClientId);
        Task SetUrl(string id, string url);
        //Task<string> ClaimGiftCoins(string id, bool isNewUser, string claimingClientId);
        Task ReturnCoinsToSender();
        Task<IReferralLink> GetReferralLinkById(string id);
        Task<IReferralLink> GetReferralLinkByUrl(string url);
        Task<bool> IsReferralLinksNumberLimitReached(string senderClientId);
    }
}
