using System.Threading.Tasks;
using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using Lykke.blue.Service.ReferralLinks.Core.Services;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Lykke.blue.Service.ReferralLinks.Core.Settings;
using AutoMapper;
using Lykke.blue.Service.ReferralLinks.Services.Domain;
using System;
using Lykke.blue.Service.ReferralLinks.Core.Settings.ServiceSettings;
using Lykke.blue.Service.ReferralLinks.Core.Domain.Requests;
using Lykke.blue.Service.ReferralLinks.Services.ExchangeOperations;
using Lykke.Service.ExchangeOperations.Client;
using System.Transactions;
using Common.Log;
using Common;

namespace Lykke.blue.Service.ReferralLinks.Services
{
    public class ReferralLinksService : IReferralLinksService
    {
        private readonly ReferralLinksSettings _settings;
        private readonly IReferralLinkRepository _referralLinkRepository;
        private readonly IFirebaseService _firebaseService;
        private readonly ExchangeService _exchangeService;
        private readonly ILog _log;
        private readonly ReferralLinkClaimsService _referralLinkClaimsService;

        public ReferralLinksService(
            IReferralLinkRepository referralLinkRepository, 
            IFirebaseService firebaseService,
            ReferralLinksSettings settings,
            ExchangeService exchangeService,
            ReferralLinkClaimsService referralLinkClaimsService,
            ILog log)
        {
            _referralLinkRepository = referralLinkRepository;
            _firebaseService = firebaseService;
            _settings = settings;
            _exchangeService = exchangeService;
            _log = log;
            _referralLinkClaimsService = referralLinkClaimsService;
        }

        public async Task<IReferralLink> CreateInvitationLink(InvitationReferralLinkRequest referralLinkRequest)
        {
            var entity = new ReferralLink();
            entity.SenderClientId = referralLinkRequest.SenderClientId;
            entity.Type = referralLinkRequest.Type.ToString();
            entity.Id = Guid.NewGuid().ToString();
            entity.ExpirationDate = null;
            entity.Amount = _settings.InvitationLinkSettings.RewardAmount;
            entity.Asset = _settings.InvitationLinkSettings.RewardAsset;
            entity.Url = await _firebaseService.GenerateUrl(entity.Id);
            entity.State = ReferralLinkState.Created.ToString();

            return await _referralLinkRepository.Create(entity);          
        }

        public async Task<IReferralLink> CreateGiftCoinsLink(GiftCoinsReferralLinkRequest referralLinkRequest)
        {
            var entity = new ReferralLink();
            entity.Id = Guid.NewGuid().ToString();
            entity.ExpirationDate = DateTime.UtcNow.AddDays(_settings.GiftCoinsLinkSettings.ExpirationDaysLimit); ;
            entity.Url = await _firebaseService.GenerateUrl(entity.Id);
            entity.SenderClientId = referralLinkRequest.SenderClientId;
            entity.Asset = referralLinkRequest.Asset;
            entity.Amount = referralLinkRequest.Amount;
            entity.Type = referralLinkRequest.Type.ToString();
            entity.State = ReferralLinkState.Created.ToString();

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

        public async Task<IEnumerable<IReferralLink>> GetReferralLinksBySenderId(string senderClientId)
        {
            return await _referralLinkRepository.GetReferralLinksBySenderId(senderClientId);
        }

        public async Task<bool> IsInvitationLinksMaxNumberReachedForSender(string senderClientId)
        {
            return await _referralLinkRepository.IsInvitationLinksMaxNumberReachedForSender(senderClientId);
        }

        public async Task SetUrl(string id, string url)
        {
            await _referralLinkRepository.SetUrl(id, url);
        }

        public async Task UpdateState(string id, ReferralLinkState state)
        {
            await _referralLinkRepository.UpdateState(id, state);
        }

        private Task Validate(IReferralLink referralLink)
        {
            //TODO: Add validation here and throw ValidationException with detailed message what is not valid
            throw new ValidationException("Not implemented, yet");
        }

        public async Task<IReferralLink> UpdateAsync(IReferralLink referralLink)
        {
            return await _referralLinkRepository.UpdateAsync(referralLink);
        }

        public async Task ReturnCoinsToSenderForExpiredGiftCoins()
        {
            var expiredLink = await _referralLinkRepository.GetExpiredGiftCoinLinks();

            foreach (var expLink in expiredLink)
            {
                await _log.WriteInfoAsync(nameof(ReturnCoinsToSenderForExpiredGiftCoins), expiredLink.ToJson(), "Ref link is expired coins should be returned to sender");
                expLink.State = ReferralLinkState.Expired.ToString();
                await _referralLinkRepository.UpdateAsync(expLink);

                try
                {
                    using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        try
                        {
                            var claim = await _referralLinkClaimsService.CreateAsync(new ReferralLinkClaim
                            {
                                IsNewClient = false,
                                RecipientClientId = expLink.SenderClientId,
                                ReferralLinkId = expLink.Id,
                                ShouldReceiveReward = false
                            });

                            expLink.State = ReferralLinkState.CoinsReturnedToSender.ToString();
                            await _referralLinkRepository.UpdateAsync(expLink);

                            var coinsRetured = await _exchangeService.TransferRewardCoins(expLink, false, expLink.SenderClientId, nameof(ReturnCoinsToSenderForExpiredGiftCoins));
                            if (coinsRetured.IsOk())
                            {
                                scope.Complete();
                                await _log.WriteInfoAsync(nameof(ReturnCoinsToSenderForExpiredGiftCoins), expiredLink.ToJson(), "Ref link was expired and coins returned to sender");
                            }
                        }
                        catch (TransactionAbortedException ex)
                        {
                            await _log.WriteErrorAsync(nameof(ReturnCoinsToSenderForExpiredGiftCoins), (new { expLink }).ToJson(), ex);
                        }
                        catch (ApplicationException ex)
                        {
                            await _log.WriteErrorAsync(nameof(ReturnCoinsToSenderForExpiredGiftCoins), (new { expLink }).ToJson(), ex);
                        }
                        catch (Exception ex)
                        {
                            await _log.WriteErrorAsync(nameof(ReturnCoinsToSenderForExpiredGiftCoins), (new { expLink }).ToJson(), ex);
                        }
                    }             
                }
                catch (Exception ex)
                {
                    await _log.WriteErrorAsync(nameof(ReturnCoinsToSenderForExpiredGiftCoins), (new { expLink }).ToJson(), ex);
                }
            }            
        }
    }
}
