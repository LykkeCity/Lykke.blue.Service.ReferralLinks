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
        private readonly IReferralLinkClaimsRepository _referralLinkClaimsRepository;

        public ReferralLinkClaimsService(
            IReferralLinkClaimsRepository referralLinkRepository)
        {
            _referralLinkClaimsRepository = referralLinkRepository;
        }

        public async Task<IReferralLinkClaim> CreateAsync(IReferralLinkClaim referralLink)
        {
            return await _referralLinkClaimsRepository.Create(referralLink);
        }

        public Task<IReferralLinkClaim> GetAsync(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<IReferralLinkClaim>> GetRefLinkClaims(string refLinkId)
        {
            return await _referralLinkClaimsRepository.GetClaimsForRefLink(refLinkId);
        }

        public async Task<IReferralLinkClaim> UpdateAsync(IReferralLinkClaim referralLink)
        {
            return await _referralLinkClaimsRepository.Update(referralLink);
        }
    }
}
