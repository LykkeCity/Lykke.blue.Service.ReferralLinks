using Lykke.Service.ReferralLinks.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Lykke.Service.ReferralLinks.Core.Domain.ReferralLink;
using System.Threading.Tasks;

namespace Lykke.Service.ReferralLinks.Services
{
    public class ReferralLinkClaimsService : IReferralLinkClaimsService
    {
        private readonly IReferralLinkClaimsRepository _referralLinkRepository;

        public ReferralLinkClaimsService(
            IReferralLinkClaimsRepository referralLinkRepository)
        {
            _referralLinkRepository = referralLinkRepository;
        }

        public async Task<IReferralLinkClaim> CreateAsync(IReferralLinkClaim referralLink)
        {
            return await _referralLinkRepository.Create(referralLink);
        }

        public Task<IReferralLinkClaim> GetAsync(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<IReferralLinkClaim>> GetRefLinkClaims(string refLinkId)
        {
            return await _referralLinkRepository.GetClaimsForRefLink(refLinkId);
        }

        public Task<IReferralLinkClaim> UpdateAsync(IReferralLinkClaim referralLink)
        {
            throw new NotImplementedException();
        }
    }
}
