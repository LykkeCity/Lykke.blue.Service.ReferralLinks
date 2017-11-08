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
using Lykke.Service.ReferralLinks.Core.Domain.Requests;

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

        public async Task<IReferralLink> CreateInvitationLink(InvitationReferralLinkRequest referralLinkRequest)
        {
            var entity = Mapper.Map<InvitationReferralLinkRequest, ReferralLink>(referralLinkRequest);
            entity.Id = Guid.NewGuid().ToString();
            entity.ExpirationDate = null;
            entity.Amount = _settings.InvitationLinkSettings.RewardAmount;
            entity.Asset = _settings.InvitationLinkSettings.RewardAsset;
            entity.Url = await _firebaseService.GenerateUrl(entity.Id);
            entity.State = ReferralLinkState.Created;

            return await _referralLinkRepository.Create(entity);

            //if (entity.Type == ReferralLinkType.MoneyTransfer)
            //    entity.ExpirationDate = DateTime.UtcNow.AddDays(_settings.ExpirationDaysLimit);
            //else if (referralLink.Type == ReferralLinkType.Invitation)
          
        }

        public async Task<IReferralLink> CreateMoneyTransferLink(MoneyTransferReferralLinkRequest referralLinkRequest)
        {
            var entity = Mapper.Map<MoneyTransferReferralLinkRequest, ReferralLink>(referralLinkRequest);//  new ReferralLink();
            entity.Id = Guid.NewGuid().ToString();
            entity.ExpirationDate = DateTime.UtcNow.AddDays(_settings.MoneyTransferLinkSettings.ExpirationDaysLimit); ;
            entity.Url = await _firebaseService.GenerateUrl(entity.Id);
            entity.State = ReferralLinkState.Created;

            return await _referralLinkRepository.Create(entity);

            //if (entity.Type == ReferralLinkType.MoneyTransfer)
            //    entity.ExpirationDate = DateTime.UtcNow.AddDays(_settings.ExpirationDaysLimit);
            //else if (referralLink.Type == ReferralLinkType.Invitation)

            if (String.IsNullOrEmpty(entity.Url))
            {
                entity.Url = await _firebaseService.GenerateUrl(entity.Id);
            }

            entity.State = ReferralLinkState.Created;
            return await _referralLinkRepository.Create(entity);
        }

        public async Task<IReferralLink> Get(string id)
        {
            return await _referralLinkRepository.Get(id);
        }

        public async Task<IReferralLink> GetReferralLinkByUrl(string url)
        {
            return await _referralLinkRepository.GetReferalLinkByUrl(url);
        }

        public async Task<IReferralLink> GetReferralLinkById(string id)
        {
            return await _referralLinkRepository.Get(id);
        }
        
        public async Task<IEnumerable<IReferralLink>> GetReferralLinksBySenderClientIdAndOrStatus(string clientId, ReferralLinkState state)
        {
            return await _referralLinkRepository.Get(clientId, state);
        }

        public async Task<IReferralLinksStatistics> GetReferralLinksStatisticsBySenderId(string senderClientId)
        {
            return await _referralLinkRepository.GetReferralLinksStatisticsBySenderId(senderClientId);
        }

        public async Task<bool> IsInvitationLinksMaxNumberReachedForSender(string senderClientId)
        {
            return await _referralLinkRepository.IsInvitationLinksMaxNumberReachedForSender(senderClientId);
        }

        public async Task ReturnCoinsToSender()
        {
            await _referralLinkRepository.ReturnCoinsToSender();
        }

        public async Task SetUrl(string id, string url)
        {
            await _referralLinkRepository.SetUrl(id, url);
        }

        public async Task UpdateState(string id, ReferralLinkState state)
        {
            await _referralLinkRepository.UpdateState(id, state);
        }

        private async Task Validate(IReferralLink referralLink)
        {
            //TODO: Add validation here and throw ValidationException with detailed message what is not valid
            throw new ValidationException("Not implemented, yet");
        }

        public async Task<IReferralLink> UpdateAsync(IReferralLink referralLink)
        {
            return await _referralLinkRepository.UpdateAsync(referralLink);
        }
    }
}
