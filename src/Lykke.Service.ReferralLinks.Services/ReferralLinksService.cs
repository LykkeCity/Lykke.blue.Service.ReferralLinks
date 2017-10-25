using System.Threading.Tasks;
using Lykke.Service.ReferralLinks.Core.Domain.ReferralLink;
using Lykke.Service.ReferralLinks.Core.Services;
using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.ReferralLinks.Services
{
    public class ReferralLinksService : IReferralLinksService
    {
        private readonly IReferralLinkRepository _referralLinkRepository;

        public ReferralLinksService(IReferralLinkRepository referralLinkRepository)
        {
            _referralLinkRepository = referralLinkRepository;
        }

        public async Task<IReferralLink> Create(IReferralLink referralLink)
        {
            return await _referralLinkRepository.Create(referralLink);
        }

        private async Task Validate(IReferralLink referralLink)
        {
            //TODO: Add validation here and throw ValidationException with detailed message what is not valid
            throw new ValidationException("Not implemented, yet");
        }
    }
}
