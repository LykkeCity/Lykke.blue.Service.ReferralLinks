// ReSharper disable ClassNeverInstantiated.Global
using Common;
using Common.Log;
using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink.Requests;
using Lykke.blue.Service.ReferralLinks.Core.Services;
using Lykke.blue.Service.ReferralLinks.Core.Settings.ServiceSettings;
using Lykke.blue.Service.ReferralLinks.Services.Domain;
using Lykke.blue.Service.ReferralLinks.Services.ExchangeOperations;
using Lykke.Service.Balances.Client;
using Lykke.Service.ExchangeOperations.Client;
using Lykke.Service.ExchangeOperations.Client.AutorestClient.Models;
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
        private readonly IBalancesClient _balancesClient;
        private readonly ExchangeService _exchangeService;
        private readonly ILog _log;
        private readonly IReferralLinkClaimsService _referralLinkClaimsService;

        public ReferralLinksService(
            IReferralLinkRepository referralLinkRepository, 
            IFirebaseService firebaseService,
            ReferralLinksSettings settings,
            IBalancesClient balancesClient,
            ILog log,
            ExchangeService exchangeService,
            IReferralLinkClaimsService referralLinkClaimsService)
        {
            _referralLinkRepository = referralLinkRepository;
            _firebaseService = firebaseService;
            _settings = settings;
            _balancesClient = balancesClient;
            _log = log;
            _exchangeService = exchangeService;
            _referralLinkClaimsService = referralLinkClaimsService;
        }

        public async Task<IReferralLink> CreateInvitationLink(InvitationReferralLinkRequest req)
        {
            var entity = ReferralLink.Create(req.SenderClientId, _settings.InvitationLinkSettings.RewardAsset, _settings.InvitationLinkSettings.RewardAmount, ReferralLinkType.Invitation);
            entity.Url = await _firebaseService.GenerateUrl(entity.Id);
            return await _referralLinkRepository.Create(entity);
        }

        public async Task<IReferralLink> CreateGiftCoinLink(string senderId, string assetId, double amount)
        {
            var entity = ReferralLink.Create(senderId, assetId, amount, ReferralLinkType.GiftCoins, null, DateTime.UtcNow.AddDays(_settings.GiftCoinsLinkSettings.ExpirationDaysLimit));
            entity.Url =  await _firebaseService.GenerateUrl(entity.Id);
            return await _referralLinkRepository.Create(entity);
        }


        public async Task<List<IReferralLink>> CreateGroupOfGiftCoinLinks(string senderId, string assetId, double[] linksAmounts)
        {
            var result = new List<IReferralLink>();

            foreach (var nextLinkAmount in linksAmounts)
            {
                var entity = ReferralLink.Create(senderId, assetId, nextLinkAmount, ReferralLinkType.GiftCoins, null, DateTime.UtcNow.AddDays(_settings.GiftCoinsLinkSettings.ExpirationDaysLimit));
                entity.Url = await _firebaseService.GenerateUrl(entity.Id);
                result.Add(entity);
            }

            await _referralLinkRepository.CreateGroup(result);

            return result.ToList();
        }

        public async Task<IReferralLink> Get(string id)
        {
            return await _referralLinkRepository.Get(id);
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

        //throws exception if refLink is already being claimed or the record is stale
        public async Task<IReferralLink> UpdateAsyncWithETagCheck(IReferralLink referralLink)
        {
            return await _referralLinkRepository.UpdateAsyncWithETagCheck(referralLink);
        }

        private async Task ReturnGiftCoinsToSender(IReferralLink expLink)
        {
            ExchangeOperationResult coinsReturned =  await _exchangeService.TransferFromSharedWallet(expLink, expLink.SenderClientId, nameof(ReturnGiftCoinsToSender));

            if (coinsReturned.IsOk())
            {
                expLink.State = ReferralLinkState.CoinsReturnedToSender.ToString();
                await _referralLinkRepository.UpdateAsync(expLink);

                await _referralLinkClaimsService.CreateAsync(new ReferralLinkClaim
                {
                    IsNewClient = false,
                    RecipientClientId = expLink.SenderClientId,
                    ReferralLinkId = expLink.Id,
                    ShouldReceiveReward = false,
                    RecipientTransactionId = coinsReturned.TransactionId,
                });

                await _log.WriteInfoAsync(nameof(ReturnGiftCoinsToSender), expLink.ToJson(), "Ref link is expired and coins have been returned to sender");
            }
            else
            {
                await _log.WriteWarningAsync(nameof(ReturnGiftCoinsToSender), expLink.ToJson(), $"{coinsReturned.Message}");
            }
        }

        public async Task CheckForExpiredGiftCoinLink()
        {
            var expiredLinks = await _referralLinkRepository.GetExpiredGiftCoinLinks();

            foreach (var expLink in expiredLinks)
            {
                await _log.WriteInfoAsync(nameof(CheckForExpiredGiftCoinLink), expLink.ToJson(), "Ref link is expired coins should be returned to sender");

                try
                {
                    expLink.State = ReferralLinkState.Expired.ToString();
                    await _referralLinkRepository.UpdateAsyncWithETagCheck(expLink);
                    await ReturnGiftCoinsToSender(expLink);
                }
                catch (Exception ex)
                {
                    await _log.WriteErrorAsync(nameof(CheckForExpiredGiftCoinLink), (new { expLink }).ToJson(), ex);
                }
            }           
        }

        public async Task<bool> HasEnoughBalance(string clientId, string assetId, double amount)
        {
            var clientBalances = await _balancesClient.GetClientBalances(clientId);

            var balance = clientBalances?.FirstOrDefault(x => x.AssetId == assetId)?.Balance;

            if (!balance.HasValue || balance.Value < amount)
            {
                await _log.WriteWarningAsync("", nameof(HasEnoughBalance), $"Balance not enough or cant get clientBalance of asset {assetId} for client id {clientId}.") ;
                return false;
            }

            return true;
        }

        public async Task<IEnumerable<IReferralLink>> GetGiftCoinLinksBySenderId(string senderClientId)
        {
            return (await _referralLinkRepository.GetReferralLinksBySenderId(senderClientId)).Where(r => r.Type == ReferralLinkType.GiftCoins.ToString());
        }
    }
}
