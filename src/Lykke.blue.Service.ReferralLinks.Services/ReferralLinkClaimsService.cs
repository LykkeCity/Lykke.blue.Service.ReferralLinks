using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using Lykke.blue.Service.ReferralLinks.Core.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.Services
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

        public async Task<IEnumerable<IReferralLinkClaim>> GetRefLinkClaims(string refLinkId)
        {
            return await _referralLinkClaimsRepository.GetClaimsForRefLinks(new[] { refLinkId });
        }

        public async Task<IReferralLinkClaim> UpdateAsync(IReferralLinkClaim referralLink)
        {
            return await _referralLinkClaimsRepository.Update(referralLink);
        }
    }
}
