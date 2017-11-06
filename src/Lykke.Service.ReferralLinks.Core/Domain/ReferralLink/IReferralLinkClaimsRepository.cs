using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.ReferralLinks.Core.Domain.ReferralLink
{
    public interface IReferralLinkClaimsRepository
    {
        Task<IReferralLinkClaim> Create(IReferralLinkClaim referralLinkClaims);
        Task<IReferralLinkClaim> Get(string id);
        Task<IReferralLinkClaim> Update(IReferralLinkClaim referralLinkClaims);
        Task Delete(string id);
    }
}
