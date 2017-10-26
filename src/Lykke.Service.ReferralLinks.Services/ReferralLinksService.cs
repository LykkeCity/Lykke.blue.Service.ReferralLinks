using System.Threading.Tasks;
using Lykke.Service.ReferralLinks.Core.Domain.ReferralLink;
using Lykke.Service.ReferralLinks.Core.Services;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

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

        public async Task<IReferralLink> Get(string id)
        {
            return await _referralLinkRepository.Get(id);
        }

        public async Task<IEnumerable<IReferralLink>> GetReferralLinksBySenderClientIdAndOrStatus(string clientId, string state)
        {
            return await _referralLinkRepository.Get(clientId, state);
        }

        public async Task UpdateState(string id, string state)
        {
            await _referralLinkRepository.UpdateState(id, state);
        }

        private async Task Validate(IReferralLink referralLink)
        {
            //TODO: Add validation here and throw ValidationException with detailed message what is not valid
            throw new ValidationException("Not implemented, yet");
        }
    }
}
