using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.Core.Services
{
    public interface IReferralLinkClaimsService
    {
        Task<IReferralLinkClaim> CreateAsync(IReferralLinkClaim referralLink);
        Task<IReferralLinkClaim> UpdateAsync(IReferralLinkClaim referralLink);
        Task<IEnumerable<IReferralLinkClaim>> GetRefLinkClaimsForClient(string refLinkId, string recipientClientId);
    }
}
