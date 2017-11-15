using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.Core.Services
{
    public interface IReferralLinkClaimsService
    {
        Task<IReferralLinkClaim> CreateAsync(IReferralLinkClaim referralLink);
        Task<IReferralLinkClaim> GetAsync(string id);
        Task<IReferralLinkClaim> UpdateAsync(IReferralLinkClaim referralLink);
        Task<IEnumerable<IReferralLinkClaim>> GetRefLinkClaims(string refLinkId);
    }
}
