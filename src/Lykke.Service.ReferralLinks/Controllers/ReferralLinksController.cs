using AutoMapper;
using Common;
using Common.Log;
using Lykke.Service.Balances.Client;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ExchangeOperations.Client;
using Lykke.Service.ExchangeOperations.Client.AutorestClient.Models;
using Lykke.Service.ReferralLinks.Core.BitCoinApi.Models;
using Lykke.Service.ReferralLinks.Core.Domain.Offchain;
using Lykke.Service.ReferralLinks.Core.Domain.ReferralLink;
using Lykke.Service.ReferralLinks.Core.Domain.Requests;
using Lykke.Service.ReferralLinks.Core.Kyc;
using Lykke.Service.ReferralLinks.Core.Services;
using Lykke.Service.ReferralLinks.Core.Settings.ServiceSettings;
using Lykke.Service.ReferralLinks.Extensions;
using Lykke.Service.ReferralLinks.Models;
using Lykke.Service.ReferralLinks.Modules.Validation;
using Lykke.Service.ReferralLinks.Responses;
using Lykke.Service.ReferralLinks.Services.Domain;
using Lykke.Service.ReferralLinks.Strings;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.SwaggerGen.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Transactions;

namespace Lykke.Service.ReferralLinks.Controllers
{
    [Route("api/referralLinks")]
    [ValidateModel]
    public class ReferralLinksController : RefLinksBaseController
    {
        private readonly ILog _log;
        private readonly IReferralLinksService _referralLinksService;
        private readonly IReferralLinkClaimsService _referralLinkClaimsService;
        private readonly IStatisticsService _statisticsService;
        private readonly IClientAccountClient _clientAccountClient;
        private readonly ISrvKycForAsset _srvKycForAsset;
        private readonly IExchangeOperationsServiceClient _exchangeOperationsService;
        private readonly CachedDataDictionary<string, Lykke.Service.Assets.Client.Models.Asset> _assets;
        private readonly IBalancesClient _balancesClient;
        private readonly ReferralLinksSettings _settings;

        public ReferralLinksController(
            ILog log,
            IReferralLinksService referralLinksService,
            IClientAccountClient clientAccountClient,
            IStatisticsService statisticsService,
            CachedDataDictionary<string, Lykke.Service.Assets.Client.Models.Asset> assets,
            ISrvKycForAsset srvKycForAsset,
            IExchangeOperationsServiceClient exchangeOperationsService,
            ReferralLinksSettings settings,
            IReferralLinkClaimsService referralLinkClaimsService,
            IBalancesClient balancesClient) : base (log)
        {

            _log = log;
            _referralLinksService = referralLinksService;
            _clientAccountClient = clientAccountClient;
            _assets = assets;
            _srvKycForAsset = srvKycForAsset;
            _exchangeOperationsService = exchangeOperationsService;
            _settings = settings;
            _referralLinkClaimsService = referralLinkClaimsService;
            _balancesClient = balancesClient;
            _statisticsService = statisticsService;
        }      


        /// <summary>
        /// Get referral link.
        /// </summary>
        /// <param name="id">Id of a referral link we wanna get.</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [SwaggerOperation("GetReferralLink")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(GetReferralLinkResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Get(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return BadRequest(Phrases.InvalidRequest);
            }

            var referralLink = await _referralLinksService.Get(id);

            if (referralLink == null)
            {
                return BadRequest(Phrases.InvalidReferralLinkId);
            }

            var result = Mapper.Map<GetReferralLinkResponse>(referralLink);
            
            return Ok(result);
        }      

        /// <summary>
        /// Get referral links statistics by sender client id.
        /// </summary>
        /// <param name="senderClientId">Sender client id by which we wanna get statistics.</param>
        /// <returns></returns>
        [HttpGet("statistics/{senderClientId}")]
        [SwaggerOperation("GetReferralLinksStatisticsBySenderId")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(GetReferralLinksStatisticsBySenderIdResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetReferralLinksStatisticsBySenderId(string senderClientId)
        {
            if (String.IsNullOrEmpty(senderClientId) || await _clientAccountClient.GetClientById(senderClientId) == null)
            {
                return await LogAndReturnBadRequest(senderClientId, ControllerContext, Phrases.InvalidSenderClientId);
            }

            var referraLinksStatistics = await _statisticsService.GetStatistics(senderClientId);

            await LogInfo(senderClientId, ControllerContext, referraLinksStatistics.ToJson());

            return Ok(referraLinksStatistics);
        }
        
        /// <summary>
        /// Request money transfer referral link.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("request/giftCoinslLink")]
        [SwaggerOperation("RequestGiftCoinsReferralLink")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> RequestGiftCoinsReferralLink([FromBody] GiftCoinsReferralLinkRequest request)
        {
            if (request == null)
            {
                return await LogAndReturnBadRequest("", ControllerContext, Phrases.InvalidRequest);
            }

            if(request.Amount <= 0)
            {
                return await LogAndReturnBadRequest(request, ControllerContext, Phrases.InvalidAmount);
            }

            if (String.IsNullOrEmpty(request.SenderClientId) || await _clientAccountClient.GetClientById(request.SenderClientId) == null)
            {
                return await LogAndReturnBadRequest(request, ControllerContext, Phrases.InvalidSenderClientId);
            }

            var asset = (await _assets.GetDictionaryAsync()).Values.Where(v => v.Symbol == request.Asset).FirstOrDefault();

            if (String.IsNullOrEmpty(request.Asset) || asset == null)
            {
                return await LogAndReturnBadRequest(request, ControllerContext, Phrases.InvalidAsset);
            }

            var clientBalances = await _balancesClient.GetClientBalances(request.SenderClientId);
            var balance = clientBalances.FirstOrDefault(x => x.AssetId == asset.Id)?.Balance;

            if(!balance.HasValue || balance.Value < request.Amount)
            {
                return await LogAndReturnBadRequest(request, ControllerContext, Phrases.InvalidTreesAmount);
            }

            var referralLink = await _referralLinksService.CreateGiftCoinsLink(request);

            await LogInfo(request, ControllerContext, referralLink.ToJson());

            return Created(uri: $"api/referralLinks/{referralLink.Id}", value: referralLink);
        }
        

        /// <summary>
        /// Request invitation referral link.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("request/invitationLink")]
        [SwaggerOperation("invitationLink")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> RequestInvitationReferralLink([FromBody] InvitationReferralLinkRequest request)
        {
            if (request == null)
            {
                return await LogAndReturnBadRequest("", ControllerContext, Phrases.InvalidRequest);
            }

            if (String.IsNullOrEmpty(request.SenderClientId) || await _clientAccountClient.GetClientById(request.SenderClientId) == null)
            {
                return await LogAndReturnBadRequest(request, ControllerContext, Phrases.InvalidSenderClientId);
            }

            var referralLinksLimitReached = await _referralLinksService.IsInvitationLinksMaxNumberReachedForSender(request.SenderClientId);

            if (referralLinksLimitReached)
            {
                return await LogAndReturnBadRequest(request, ControllerContext, Phrases.ReferralLinksLimitReached);
            }

            var referralLink = await _referralLinksService.CreateInvitationLink(request);

            await LogInfo(request, ControllerContext, referralLink.ToJson());

            return Created(uri: $"api/referralLinks/{referralLink.Id}", value: referralLink);
        }

        private string ValidateClaimGiftCoinsRequest(IReferralLink refLink)
        {
            if (refLink == null)
            {
                return $"RefLink with id {refLink.Id} not found.";
            }

            if (refLink.Amount == 0)
            {
                return $"Requested amount for RefLink with id {refLink.Id} is 0 (not set). Check transfer's history.";
            }

            if (refLink.Type == ReferralLinkType.GiftCoins.ToString() && refLink.State != ReferralLinkState.SentToLykkeSharedWallet.ToString())
            {
                return $"RefLink type {refLink.Type} with state {refLink.State} can not be claimed.";
            }

            if (refLink.Type == ReferralLinkType.Invitation.ToString() && refLink.State != ReferralLinkState.Created.ToString())
            {
                return $"RefLink type {refLink.Type} with state {refLink.State} can not be claimed.";
            }

            if (refLink.ExpirationDate.HasValue && refLink.ExpirationDate.Value.CompareTo(DateTime.Now) < 0)
            {
                return $"RefLink is expired at {refLink.ExpirationDate.Value} and can not be claimed.";
            }
            
            return null;
        }       


        [HttpPost("claimGiftCoins")]
        [SwaggerOperation("ClaimGiftCoins")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> ClaimGiftCoins([FromBody] ClaimReferralLinkRequest request)
        {
            var refLink = await _referralLinksService.GetReferralLinkById(request.ReferalLinkId);

            var validationError = ValidateClaimGiftCoinsRequest(refLink);
            if (!String.IsNullOrEmpty(validationError))
            {
                return await LogAndReturnBadRequest(request, ControllerContext, validationError);
            }

            var asset = (await _assets.GetDictionaryAsync()).Values.Where(v => v.Symbol == refLink.Asset).FirstOrDefault();

            if (asset == null)
            {
                return await LogAndReturnBadRequest(request, ControllerContext, $"Asset {refLink.Asset} for Referral link {refLink.Id} not found. Check transfer's history.");
            }
            
            if (await _srvKycForAsset.IsKycNeeded(request.RecipientClientId, asset.Id))
                return await LogAndReturnBadRequest(request, ControllerContext, $"KYC needed for recipient client id {request.RecipientClientId} before claiming asset {refLink.Asset}");

            var newRefLinkClaimRecipient = await CreateNewRefLinkClaim(refLink, request.RecipientClientId, true, request.IsNewClient);

            var transactionRewardRecipient = await TransferRewardCoins(refLink, request, request.RecipientClientId);

            try
            {
                if (transactionRewardRecipient.IsOk())
                {
                    using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        await SetRefLinkClaimTransactionId(transactionRewardRecipient, newRefLinkClaimRecipient);
                        await UpdateRefLinkState(refLink, ReferralLinkState.Claimed);
                        scope.Complete();
                    }

                    return Ok(new { TransactionRewardRecipient = transactionRewardRecipient });
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.InternalServerError, new { TransactionRewardRecipientError = $"Code: {transactionRewardRecipient.Code}, Message: {transactionRewardRecipient.Message}" });
                }                
               
            }
            catch (TransactionAbortedException ex)
            {
                await LogClaimReferralLinkError(refLink, request, request.RecipientClientId, nameof(ClaimGiftCoins), ex);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { ex.Message });
            }
            catch (ApplicationException ex)
            {
                await LogClaimReferralLinkError(refLink, request, request.RecipientClientId, nameof(ClaimGiftCoins), ex);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { ex.Message });
            }
            catch (Exception ex)
            {
                await LogClaimReferralLinkError(refLink, request, request.RecipientClientId, nameof(ClaimGiftCoins), ex);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { ex.Message });
            }            
            
        }

        

        [HttpPost("claimInvitationLink")]
        [SwaggerOperation("ClaimInvitationLink")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> ClaimInvitationLink([FromBody] ClaimReferralLinkRequest request)
        {
            if (!request.IsNewClient)
            {
                return await LogAndReturnBadRequest(request, ControllerContext, "Not a new client.");
            }            

            var refLink = await _referralLinksService.GetReferralLinkById(request.ReferalLinkId) ?? await _referralLinksService.GetReferralLinkByUrl(request.ReferalLinkUrl);

            var validationError = ValidateClaimGiftCoinsRequest(refLink);
            if (!String.IsNullOrEmpty(validationError))
            {
                return await LogAndReturnBadRequest(request, ControllerContext, validationError);
            }

            var claims = await _referralLinkClaimsService.GetRefLinkClaims(refLink.Id);

            var alreadyClaimedByThisRecipient = claims.Where(c => c.RecipientClientId == request.RecipientClientId).Count() > 0;
            if (alreadyClaimedByThisRecipient)
            {
                return await LogAndReturnBadRequest(request, ControllerContext, $"Link already claimed by client id {request.RecipientClientId}");
            }

            bool shouldReceiveReward = await ShoulReceiveReward(claims, refLink);

            var newRefLinkClaimRecipient = await CreateNewRefLinkClaim(refLink, request.RecipientClientId, shouldReceiveReward, true);

            if (shouldReceiveReward)
            {
                var newRefLinkClaimSender = await CreateNewRefLinkClaim(refLink, refLink.SenderClientId, shouldReceiveReward, false);

                var transactionRewardSender = await TransferRewardCoins(refLink, request, refLink.SenderClientId);
                await SetRefLinkClaimTransactionId(transactionRewardSender, newRefLinkClaimSender);

                var transactionRewardRecipient = await TransferRewardCoins(refLink, request, request.RecipientClientId);
                await SetRefLinkClaimTransactionId(transactionRewardRecipient, newRefLinkClaimRecipient);  
                
                if(transactionRewardSender.IsOk() && transactionRewardRecipient.IsOk())
                {
                    return Ok
                        (
                            new
                            {
                                TransactionRewardSender = transactionRewardSender,
                                TransactionRewardRecipient = transactionRewardRecipient
                            }
                        );
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.InternalServerError, new { TransactionRewardSenderError = $"Code: {transactionRewardSender.Code}, Message: {transactionRewardSender.Message}" , TransactionRewardRecipientError = $"Code: {transactionRewardRecipient.Code}, Message: {transactionRewardRecipient.Message}" });
                }
            }

            return NoContent();
        }

        private async Task SetRefLinkClaimTransactionId(ExchangeOperationResult result, IReferralLinkClaim refLinkClaim)
        {
            if (result.IsOk())
            {
                refLinkClaim.RecipientTransactionId = result.TransactionId;
                await _referralLinkClaimsService.UpdateAsync(refLinkClaim);
                await LogInfo(refLinkClaim, ControllerContext, $"RefLinkClaim RecipientTransactionId set to {result.TransactionId}");
            }          
        }

        private async Task<bool> ShoulReceiveReward(IEnumerable<IReferralLinkClaim> claims, IReferralLink refLink)
        {
            var countOfNewClientClaims = claims.Where(c => c.IsNewClient && c.ShouldReceiveReward && c.RecipientClientId != refLink.SenderClientId).Count();
            bool shouldReceiveReaward = countOfNewClientClaims < _settings.InvitationLinkSettings.MaxNumOfClientsToReceiveReward;

            if (!shouldReceiveReaward)
            {
                await LogInfo(refLink, ControllerContext, $"MaxNumOfClientsToReceiveReward reached. Recipient and sender wont get reward coins.");
            }

            return shouldReceiveReaward;
        }

        private async Task<IReferralLinkClaim> CreateNewRefLinkClaim(IReferralLink refLink, string recipientClientId, bool shouldReceiveReward, bool isNewClient)
        {
            return await _referralLinkClaimsService.CreateAsync(new ReferralLinkClaim
            {
                IsNewClient = isNewClient,
                RecipientClientId = recipientClientId,
                ReferralLinkId = refLink.Id,
                ShouldReceiveReward = shouldReceiveReward
            });
        }

        private async Task UpdateRefLinkState(IReferralLink refLink, ReferralLinkState state)
        {
            refLink.State = state.ToString();
            await _referralLinksService.UpdateAsync(refLink);
            await LogInfo(refLink, ControllerContext, $"RefLink state set to {state.ToString()}");
        }

        private async Task<ExchangeOperationResult> TransferRewardCoins(IReferralLink refLink, ClaimReferralLinkRequest request, string recipientClientId)
        {
            try
            {
                var asset = (await _assets.GetDictionaryAsync()).Values.Where(v => v.Symbol == refLink.Asset).FirstOrDefault();
                if (asset == null)
                {
                    var message = $"Asset with symbol {refLink.Asset} not found";
                    await _log.WriteErrorAsync(ControllerContext.GetExecutongControllerAndAction(), nameof(TransferRewardCoins), (new { Error = message }).ToJson(), new Exception(message), DateTime.Now);
                    return new ExchangeOperationResult { Message = message };
                }                   

                var result = await _exchangeOperationsService.TransferAsync(
                         recipientClientId,
                         _settings.LykkeReferralClientId,
                         (double)refLink.Amount,
                         asset.Id,
                         TransferType.Common.ToString()
                         );

                if (!result.IsOk())
                {
                    await _log.WriteErrorAsync(ControllerContext.GetExecutongControllerAndAction(), nameof(TransferRewardCoins), (new { Error = $"TransferAsync from exchangeOperationsService returned error: Message: {result.Message}, Code: {result.Code}" }).ToJson(), new Exception(result.Message), DateTime.Now);
                }

                await LogInfo(request, ControllerContext, $"Transfer successfull: {result.ToJson()}");

                return result;
            }
            catch (OffchainException ex)
            {
                await LogClaimReferralLinkError(refLink, request, recipientClientId, nameof(TransferRewardCoins), ex);
                return new ExchangeOperationResult { };
            }
            catch (ApplicationException ex)
            {
                await LogClaimReferralLinkError(refLink, request, recipientClientId, nameof(TransferRewardCoins), ex);
                return new ExchangeOperationResult { };
            }
            catch (Exception ex)
            {
                await LogClaimReferralLinkError(refLink, request, recipientClientId, nameof(TransferRewardCoins), ex);
                return new ExchangeOperationResult { };
            }
        }

        

        private async Task LogClaimReferralLinkError(IReferralLink refLink, ClaimReferralLinkRequest claimRequest, string recipientClientId, string method, Exception ex)
        {
            await _log.WriteErrorAsync(ControllerContext.GetExecutongControllerAndAction(), method, (new { refLink, claimRequest, recipientClientId }).ToJson(), ex, DateTime.Now);            
        }
    }
}

