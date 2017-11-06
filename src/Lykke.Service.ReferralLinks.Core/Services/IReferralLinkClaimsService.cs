using Lykke.Service.ReferralLinks.Core.Domain.ReferralLink;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.ReferralLinks.Core.Services
{
    public interface IReferralLinkClaimsService
    {
        Task<IReferralLinkClaim> CreateAsync(IReferralLinkClaim referralLink);
        Task<IReferralLinkClaim> GetAsync(string id);
        Task<IReferralLinkClaim> UpdateAsync(IReferralLinkClaim referralLink);
    }
}
