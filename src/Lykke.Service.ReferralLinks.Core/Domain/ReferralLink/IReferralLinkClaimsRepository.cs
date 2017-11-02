using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.ReferralLinks.Core.Domain.ReferralLink
{
    public interface IReferralLinkClaimsRepository
    {
        Task<IReferralLinkClaims> Create(IReferralLinkClaims referralLinkClaims);
        Task<IReferralLinkClaims> Get(string id);
        Task<IReferralLinkClaims> Update(IReferralLinkClaims referralLinkClaims);
        Task Delete(string id);
    }
}
