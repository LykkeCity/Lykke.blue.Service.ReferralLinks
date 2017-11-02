using System.Threading.Tasks;
using Lykke.Service.ReferralLinks.Core.Domain.ReferralLink;
using Lykke.Service.ReferralLinks.Core.Services;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Lykke.Service.ReferralLinks.Core.Settings;
using AutoMapper;
using Lykke.Service.ReferralLinks.Services.Domain;
using System;
using Lykke.Service.ReferralLinks.Core.Settings.ServiceSettings;

namespace Lykke.Service.ReferralLinks.Services
{
    public class ReferralLinksService : IReferralLinksService
    {
        private readonly ReferralLinksSettings _settings;
        private readonly IReferralLinkRepository _referralLinkRepository;
        private readonly IFirebaseService _firebaseService;

        public ReferralLinksService(
            IReferralLinkRepository referralLinkRepository, 
            IFirebaseService firebaseService,
            ReferralLinksSettings settings)
        {
            _referralLinkRepository = referralLinkRepository;
            _firebaseService = firebaseService;
            _settings = settings;
        }

        public async Task<string> ClaimGiftCoins(string id, bool isNewUser, string claimingClientId)
        {
            return await _referralLinkRepository.ClaimGiftCoins(id, isNewUser, claimingClientId);
        }

        public async Task<IReferralLink> Create(IReferralLink referralLink)
        {
            var entity = Mapper.Map<ReferralLink>(referralLink);

            if(String.IsNullOrEmpty(entity.Id))
            {
                entity.Id = Guid.NewGuid().ToString();
            }

            if (entity.Type == ReferralLinkType.MoneyTransfer)
                entity.ExpirationDate = DateTime.UtcNow.AddDays(_settings.ExpirationDaysLimit);
            else if (referralLink.Type == ReferralLinkType.Invitation)
                entity.ExpirationDate = null;

            if (String.IsNullOrEmpty(entity.Url))
            {
                entity.Url = await _firebaseService.GenerateUrl(entity.Id);
            }

            return await _referralLinkRepository.Create(entity);
        }

        public async Task<IReferralLink> Get(string id)
        {
            return await _referralLinkRepository.Get(id);
        }

        public async Task<IEnumerable<IReferralLink>> GetReferralLinksBySenderClientIdAndOrStatus(string clientId, string state)
        {
            return await _referralLinkRepository.Get(clientId, state);
        }

        public async Task<IReferralLinksStatistics> GetReferralLinksStatisticsBySenderId(string senderClientId)
        {
            return await _referralLinkRepository.GetReferralLinksStatisticsBySenderId(senderClientId);
        }

        public async Task<bool> IsReferralLinksNumberLimitReached(string senderClientId)
        {
            return await _referralLinkRepository.IsReferralLinksNumberLimitReached(senderClientId);
        }

        public async Task ReturnCoinsToSender()
        {
            await _referralLinkRepository.ReturnCoinsToSender();
        }

        public async Task SetUrl(string id, string url)
        {
            await _referralLinkRepository.SetUrl(id, url);
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
