using Common;
using Common.Log;
using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink.Requests;
using Lykke.blue.Service.ReferralLinks.Core.Services;
using Lykke.blue.Service.ReferralLinks.Core.Settings.ServiceSettings;
using Lykke.blue.Service.ReferralLinks.Services.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.Services
{
    public class ReferralLinksService : IReferralLinksService
    {
        private readonly ReferralLinksSettings _settings;
        private readonly IReferralLinkRepository _referralLinkRepository;
        private readonly IFirebaseService _firebaseService;
        private readonly ILog _log;

        public ReferralLinksService(
            IReferralLinkRepository referralLinkRepository, 
            IFirebaseService firebaseService,
            ReferralLinksSettings settings,
            ILog log)
        {
            _referralLinkRepository = referralLinkRepository;
            _firebaseService = firebaseService;
            _settings = settings;
            _log = log;
        }

        public async Task<IReferralLink> CreateInvitationLink(InvitationReferralLinkRequest referralLinkRequest)
        {
            var entity = new ReferralLink
            {
                SenderClientId = referralLinkRequest.SenderClientId,
                Type = referralLinkRequest.Type.ToString(),
                Id = Guid.NewGuid().ToString(),
                ExpirationDate = null,
                Amount = _settings.InvitationLinkSettings.RewardAmount,
                Asset = _settings.InvitationLinkSettings.RewardAsset,
                CreatedAt = DateTime.UtcNow
            };
            entity.Url = await _firebaseService.GenerateUrl(entity.Id);
            entity.State = ReferralLinkState.Created.ToString();

            return await _referralLinkRepository.Create(entity);          
        }

        public async Task<IReferralLink> CreateGiftCoinLink(GiftCoinsReferralLinkRequest referralLinkRequest)
        {
            var entity = new ReferralLink
            {
                Id = Guid.NewGuid().ToString(),
                ExpirationDate = DateTime.UtcNow.AddDays(_settings.GiftCoinsLinkSettings.ExpirationDaysLimit)
            };
            entity.Url = await _firebaseService.GenerateUrl(entity.Id);
            entity.SenderClientId = referralLinkRequest.SenderClientId;
            entity.Asset = referralLinkRequest.Asset;
            entity.Amount = referralLinkRequest.Amount;
            entity.Type = referralLinkRequest.Type.ToString();
            entity.State = ReferralLinkState.Created.ToString();
            entity.CreatedAt = DateTime.UtcNow;

            return await _referralLinkRepository.Create(entity);
        }


        public async Task<string> CreateGroupOfGiftCoinLinks(GroupGiftCoinLinkRequest referralLinkRequest)
        {
            var result = new List<IReferralLink>();

            foreach (var nextLinkAmount in referralLinkRequest.AmountForEachLink)
            {
                var entity = new ReferralLink
                {
                    Id = Guid.NewGuid().ToString(),
                    ExpirationDate = DateTime.UtcNow.AddDays(_settings.GiftCoinsLinkSettings.ExpirationDaysLimit)
                };
                entity.Url = await _firebaseService.GenerateUrl(entity.Id);
                entity.SenderClientId = referralLinkRequest.SenderClientId;
                entity.Asset = referralLinkRequest.Asset;
                entity.Amount = nextLinkAmount;
                entity.Type = referralLinkRequest.Type.ToString();
                entity.State = ReferralLinkState.Created.ToString();
                entity.CreatedAt = DateTime.UtcNow;

                result.Add(entity);
            }

            return await _referralLinkRepository.CreateGroup(result);
        }

        public async Task<IEnumerable<IReferralLink>> GetGroupBySenderId(string senderId)
        {
            return await _referralLinkRepository.GetGroupBySenderId(senderId);
        }

        public async Task<IReferralLink> Get(string id)
        {
            return await _referralLinkRepository.Get(id);
        }

        public async Task<IEnumerable<IReferralLink>> GetGroup(string groupId)
        {
            return await _referralLinkRepository.GetGroup(groupId);
        }

        public async Task<IReferralLink> GetReferralLinkByUrl(string url)
        {
            return await _referralLinkRepository.GetReferalLinkByUrl(url);
        }
       
        public IEnumerable<IReferralLink> GetInvitationLinksForSenderId(string senderClientId)
        {
            return (_referralLinkRepository.GetReferralLinksBySenderId(senderClientId)).Result.Where(r=>r.Type == ReferralLinkType.Invitation.ToString());
        }

        public async Task<IReferralLink> UpdateAsync(IReferralLink referralLink)
        {
            return await _referralLinkRepository.UpdateAsync(referralLink);
        }

        public async Task<IReferralLink> UpdateAsyncWithETagCheck(IReferralLink referralLink)
        {
            return await _referralLinkRepository.UpdateAsyncWithETagCheck(referralLink);
        }

        public async Task CheckForExpiredGiftCoinLink()
        {
            var expiredLink = await _referralLinkRepository.GetExpiredGiftCoinLinks();

            foreach (var expLink in expiredLink)
            {
                await _log.WriteInfoAsync(nameof(CheckForExpiredGiftCoinLink), expiredLink.ToJson(), "Ref link is expired coins should be returned to sender");

                //ExchangeOperationResult coinsRetured = new ExchangeOperationResult();

                try
                {
                    expLink.State = ReferralLinkState.Expired.ToString();
                    await _referralLinkRepository.UpdateAsync(expLink);

                    // Code below is for returning coins to sender, but it may be better to be called manually. It should be extracted to a separated method

                    //coinsRetured = await _exchangeService.TransferRewardCoins(expLink, false, expLink.SenderClientId, nameof(ReturnCoinsToSenderForExpiredGiftCoins));
                    //if (coinsRetured.IsOk())
                    //{
                    //    var claim = await _referralLinkClaimsService.CreateAsync(new ReferralLinkClaim
                    //    {
                    //        IsNewClient = false,
                    //        RecipientClientId = expLink.SenderClientId,
                    //        ReferralLinkId = expLink.Id,
                    //        ShouldReceiveReward = false,
                    //        RecipientTransactionId = coinsRetured.TransactionId,
                    //    });

                    //    expLink.State = ReferralLinkState.CoinsReturnedToSender.ToString();
                    //    await _referralLinkRepository.UpdateAsync(expLink);

                    //    await _log.WriteInfoAsync(nameof(ReturnCoinsToSenderForExpiredGiftCoins), expiredLink.ToJson(), "Ref link was expired and coins have been returned to sender");
                    //}
                    //else
                    //{
                    //    await _log.WriteWarningAsync(nameof(ReturnCoinsToSenderForExpiredGiftCoins), expiredLink.ToJson(), $"_exchangeService.TransferRewardCoins failed: Code={coinsRetured.Code}, Message={coinsRetured.Message}, TransactionId={coinsRetured.TransactionId}");
                    //} 
                }
                catch (Exception ex)
                {
                    await _log.WriteErrorAsync(nameof(CheckForExpiredGiftCoinLink), (new { expLink }).ToJson(), ex);
                }
            }           
        }
    }
}
