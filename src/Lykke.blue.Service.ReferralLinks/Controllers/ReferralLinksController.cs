using AutoMapper;
using Common;
using Common.Log;
using Lykke.Service.Balances.Client;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ExchangeOperations.Client;
using Lykke.Service.ExchangeOperations.Client.AutorestClient.Models;
using Lykke.blue.Service.ReferralLinks.Core.BitCoinApi.Models;
using Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain;
using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using Lykke.blue.Service.ReferralLinks.Core.Domain.Requests;
using Lykke.blue.Service.ReferralLinks.Core.Kyc;
using Lykke.blue.Service.ReferralLinks.Core.Services;
using Lykke.blue.Service.ReferralLinks.Core.Settings.ServiceSettings;
using Lykke.blue.Service.ReferralLinks.Extensions;
using Lykke.blue.Service.ReferralLinks.Models;
using Lykke.blue.Service.ReferralLinks.Modules.Validation;
using Lykke.blue.Service.ReferralLinks.Responses;
using Lykke.blue.Service.ReferralLinks.Services.Domain;
using Lykke.blue.Service.ReferralLinks.Strings;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.SwaggerGen.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Transactions;
using Lykke.blue.Service.ReferralLinks.Models.RefLinkResponseModels;
using Lykke.blue.Service.ReferralLinks.Requests;
using Lykke.blue.Service.ReferralLinks.Services.ExchangeOperations;

namespace Lykke.blue.Service.ReferralLinks.Controllers
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
        private readonly ExchangeService _exchangeService;
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
            ExchangeService exchangeService,
            IBalancesClient balancesClient) : base (log)
        {

            _log = log;
            _referralLinksService = referralLinksService;
            _clientAccountClient = clientAccountClient;
            _assets = assets;
            _srvKycForAsset = srvKycForAsset;
            _exchangeService = exchangeService;
            _settings = settings;
            _referralLinkClaimsService = referralLinkClaimsService;
            _balancesClient = balancesClient;
            _statisticsService = statisticsService;
        }      


        /// <summary>
        /// Get referral link by id.
        /// </summary>
        /// <param name="id">Id of a referral link we wanna get.</param>
        /// <returns></returns>
        [HttpGet("id/{id}")]
        [SwaggerOperation("GetReferralLinkById")]
        [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(GetReferralLinkResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetReferralLinkById(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return NotFound(ErrorResponseModel.Create("Requested id cant be empty"));
            }

            var referralLink = await _referralLinksService.Get(id);

            if (referralLink == null)
            {
                var msg = $"Ref link with id {id} does not exist";
                await LogWarn(id, ControllerContext, msg);
                return NotFound(ErrorResponseModel.Create(msg));
            }

            var result = Mapper.Map<GetReferralLinkResponse>(referralLink);
            
            return Ok(result);
        }

        /// <summary>
        /// Get referral link by url.
        /// </summary>
        /// <param name="url">Url of the referral link we want to get.</param>
        /// <returns></returns>
        [HttpGet("url/{url}")]
        [SwaggerOperation("GetReferralLinkByUrl")]
        [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(GetReferralLinkResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetReferralLinkByUrl(string url)
        {
            var decoded = WebUtility.UrlDecode(url);

            if (String.IsNullOrEmpty(decoded))
            {
                return NotFound(ErrorResponseModel.Create("Requested url cant be empty"));
            }

            var referralLink = await _referralLinksService.GetReferralLinkByUrl(decoded);

            if (referralLink == null)
            {
                var msg = $"Ref link with url {decoded} does not exist";
                await LogWarn(decoded, ControllerContext, msg);
                return NotFound(ErrorResponseModel.Create(msg));
            }

            var result = Mapper.Map<GetReferralLinkResponse>(referralLink);

            return Ok(result);
        }

        /// <summary>
        /// Get referral links statistics by sender client id.
        /// </summary>
        /// <param name="request">Sender client id by which we wanna get statistics.</param>
        /// <returns></returns>
        [HttpPost("statistics")]
        [SwaggerOperation("GetReferralLinksStatisticsBySenderId")]
        [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(GetReferralLinksStatisticsBySenderIdResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetReferralLinksStatisticsBySenderId([FromBody] RefLinkStatisticsRequest request)
        {
            if (String.IsNullOrEmpty(request.SenderClientId) || await _clientAccountClient.GetClientById(request.SenderClientId) == null)
            {
                return await LogAndReturnBadRequest(request.SenderClientId, ControllerContext, Phrases.InvalidSenderClientId);
            }

            var referraLinksStatistics = await _statisticsService.GetStatistics(request.SenderClientId);

            await LogInfo(request.SenderClientId, ControllerContext, referraLinksStatistics.ToJson());

            return Ok(referraLinksStatistics);
        }

        /// <summary>
        /// Request money transfer referral link   
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("request/giftCoinslLink")]
        [SwaggerOperation("RequestGiftCoinsReferralLink")]
        [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(RequestRefLinkResponse), (int)HttpStatusCode.Created)]        
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

            if (clientBalances == null)
            {
                return await LogAndReturnNotFound(request, ControllerContext, $"Cant get clientBalance of asset {asset.Symbol} for client id {request.SenderClientId} from service {_settings.ExternalServices.BalancesServiceUrl}");
            }

            var balance = clientBalances.FirstOrDefault(x => x.AssetId == asset.Id)?.Balance;

            if (!balance.HasValue || balance.Value < request.Amount)
            {
                return await LogAndReturnBadRequest(request, ControllerContext, Phrases.InvalidTreesAmount);
            }

            var referralLink = await _referralLinksService.CreateGiftCoinsLink(request);

            await LogInfo(request, ControllerContext, referralLink.ToJson());

            return Created(uri: $"api/referralLinks/{referralLink.Id}", value: new RequestRefLinkResponse { RefLinkId = referralLink.Id, RefLinkUrl = referralLink.Url });
        }
        

        /// <summary>
        /// Request invitation referral link.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("request/invitationLink")]
        [SwaggerOperation("RequestInvitationReferralLink")]
        [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(RequestRefLinkResponse), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(RequestRefLinkResponse), (int)HttpStatusCode.OK)]
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

            var invitationLinkAlreadyCreated = await _referralLinksService.GetInvitationLinksBySenderId(request.SenderClientId);
            if (invitationLinkAlreadyCreated != null)
            {

                await LogInfo(request, ControllerContext, $"Invitation link already exists for SenderClientId {request.SenderClientId}. RefLinkId {invitationLinkAlreadyCreated.Id}");
                return Ok(new RequestRefLinkResponse { RefLinkUrl = invitationLinkAlreadyCreated.Url, RefLinkId = invitationLinkAlreadyCreated.Id });
            }           

            var referralLink = await _referralLinksService.CreateInvitationLink(request);

            await LogInfo(request, ControllerContext, referralLink.ToJson());

            return Created(uri: $"api/referralLinks/{referralLink.Id}", value: new RequestRefLinkResponse { RefLinkUrl = referralLink.Url, RefLinkId = referralLink.Id } );
        }

        private async Task<string> ValidateClaimRefLinkAndRequest(IReferralLink refLink, ClaimReferralLinkRequest request)
        {
            if (request.RecipientClientId == null || await _clientAccountClient.GetClientById(request.RecipientClientId) == null)
            {
                return $"RecipientClientId not found or not supplied";
            }
            if (request.ReferalLinkId == null && request.ReferalLinkUrl == null)
            {
                return $"ReferalLinkId and ReferalLinkUrl not supplied. Please specify either of them.";
            }

            if (refLink == null)
            {
                return $"RefLink not found by id or url.";
            }

            if (refLink.SenderClientId == request.RecipientClientId)
            {
                return $"RecipientClientId can't be the same as SenderClientId. Client cant claim their own ref link.";
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
        [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ClaimRefLinkResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> ClaimGiftCoins([FromBody] ClaimReferralLinkRequest request)
        {
            var refLink = await _referralLinksService.GetReferralLinkById(request.ReferalLinkId ?? "");

            var validationError = await ValidateClaimRefLinkAndRequest(refLink, request);
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

            var transactionRewardRecipient = await _exchangeService.TransferRewardCoins(refLink, request.IsNewClient, request.RecipientClientId, ControllerContext.GetControllerAndAction());

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

                    return Ok(new ClaimRefLinkResponse { TransactionRewardRecipient = transactionRewardRecipient.TransactionId, SenderOffchainTransferId =  refLink.SenderOffchainTransferId });
                }
                else
                {
                    return await LogAndReturnNotFound(request, ControllerContext, $"TransactionRewardRecipientError: Code: {transactionRewardRecipient.Code}, Message: {transactionRewardRecipient.Message}");
                }                
               
            }
            catch (TransactionAbortedException ex)
            {
                await LogError(new { Request = request, RefLink = refLink ?? new ReferralLink(), }, ControllerContext, ex) ;
                return NotFound(ex.Message);
            }
            catch (ApplicationException ex)
            {
                await LogError(new { Request = request, RefLink = refLink ?? new ReferralLink(), }, ControllerContext, ex);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                await LogError(new { Request = request, RefLink = refLink ?? new ReferralLink(), }, ControllerContext, ex);
                return NotFound(ex.Message);
            }            
            
        }




        [HttpPost("claimInvitationLink")]
        [SwaggerOperation("ClaimInvitationLink")]
        [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ClaimRefLinkResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> ClaimInvitationLink([FromBody] ClaimReferralLinkRequest request)
        {
            try
            {
                if (!request.IsNewClient)
                {
                    return await LogAndReturnBadRequest(request, ControllerContext, "Not a new client.");
                }

                var refLink = await _referralLinksService.GetReferralLinkById(request.ReferalLinkId ?? "") ?? await _referralLinksService.GetReferralLinkByUrl(request.ReferalLinkUrl ?? "");

                var validationError = await ValidateClaimRefLinkAndRequest(refLink, request);
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

                    var transactionRewardSender = await _exchangeService.TransferRewardCoins(refLink, false, refLink.SenderClientId, ControllerContext.GetControllerAndAction());
                    await SetRefLinkClaimTransactionId(transactionRewardSender, newRefLinkClaimSender);

                    var transactionRewardRecipient = await _exchangeService.TransferRewardCoins(refLink, request.IsNewClient, request.RecipientClientId, ControllerContext.GetControllerAndAction());
                    await SetRefLinkClaimTransactionId(transactionRewardRecipient, newRefLinkClaimRecipient);

                    if (transactionRewardSender.IsOk() && transactionRewardRecipient.IsOk())
                    {
                        return Ok
                            (
                                new ClaimRefLinkResponse
                                {
                                    TransactionRewardSender = transactionRewardSender.TransactionId,
                                    TransactionRewardRecipient = transactionRewardRecipient.TransactionId
                                }
                            );
                    }
                    else
                    {
                        return await LogAndReturnNotFound(request, ControllerContext, $"TransactionRewardRecipientError: Code: {transactionRewardRecipient.Code}, {transactionRewardRecipient.Message}; TransactionRewardSenderError: Code: {transactionRewardSender.Code}, {transactionRewardSender.Message}");
                    }
                }

                return Ok(new ClaimRefLinkResponse ());
            }
            catch (Exception ex)
            {
                await LogError(request, ControllerContext, ex);
                return NotFound(ErrorResponseModel.Create($"{ex.Message}.{ex.InnerException?.Message}."));
            }

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

        
    }
}

