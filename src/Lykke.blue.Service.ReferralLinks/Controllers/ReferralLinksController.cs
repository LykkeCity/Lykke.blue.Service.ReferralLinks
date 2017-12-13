using AutoMapper;
using Common;
using Common.Log;
using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink.Requests;
using Lykke.blue.Service.ReferralLinks.Core.Kyc;
using Lykke.blue.Service.ReferralLinks.Core.Services;
using Lykke.blue.Service.ReferralLinks.Core.Settings.ServiceSettings;
using Lykke.blue.Service.ReferralLinks.Extensions;
using Lykke.blue.Service.ReferralLinks.Models;
using Lykke.blue.Service.ReferralLinks.Models.RefLinkResponseModels;
using Lykke.blue.Service.ReferralLinks.Modules.Validation;
using Lykke.blue.Service.ReferralLinks.Requests;
using Lykke.blue.Service.ReferralLinks.Responses;
using Lykke.blue.Service.ReferralLinks.Services.Domain;
using Lykke.blue.Service.ReferralLinks.Services.ExchangeOperations;
using Lykke.blue.Service.ReferralLinks.Strings;
using Lykke.Service.Balances.Client;
using Lykke.Service.ExchangeOperations.Client;
using Lykke.Service.ExchangeOperations.Client.AutorestClient.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Transactions;

namespace Lykke.blue.Service.ReferralLinks.Controllers
{
    [Route("api/referralLinks")]
    [ValidateModel]
    public class ReferralLinksController : RefLinksBaseController
    {
        private readonly IReferralLinksService _referralLinksService;
        private readonly IReferralLinkClaimsService _referralLinkClaimsService;
        private readonly IStatisticsService _statisticsService;
        private readonly ISrvKycForAsset _srvKycForAsset;
        private readonly ExchangeService _exchangeService;
        private readonly CachedDataDictionary<string, Lykke.Service.Assets.Client.Models.Asset> _assets;
        private readonly IBalancesClient _balancesClient;
        private readonly ReferralLinksSettings _settings;
        private readonly double MinimalAmount = 0.00000001;

        public ReferralLinksController(
            ILog log,
            IReferralLinksService referralLinksService,
            IStatisticsService statisticsService,
            CachedDataDictionary<string, Lykke.Service.Assets.Client.Models.Asset> assets,
            ISrvKycForAsset srvKycForAsset,
            IExchangeOperationsServiceClient exchangeOperationsService,
            ReferralLinksSettings settings,
            IReferralLinkClaimsService referralLinkClaimsService,
            ExchangeService exchangeService,
            IBalancesClient balancesClient) : base(log)
        {

            _referralLinksService = referralLinksService;
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
        [HttpGet("{id}")]
        [SwaggerOperation("GetReferralLinkById")]
        [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(GetReferralLinkResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetReferralLinkById(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return await LogAndReturnNotFound(id, ControllerContext, "Requested id cant be empty");
            }

            var referralLink = await _referralLinksService.Get(id);

            if (referralLink == null)
            {
                var msg = $"Ref link with id {id} does not exist";
                return await LogAndReturnNotFound(id, ControllerContext, msg);
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
            if (String.IsNullOrEmpty(request.SenderClientId))
            {
                return await LogAndReturnBadRequest(request.SenderClientId, ControllerContext, Phrases.InvalidSenderClientId);
            }

            var referraLinksStatistics = await _statisticsService.GetStatistics(request.SenderClientId);

            await LogInfo(request.SenderClientId, ControllerContext, referraLinksStatistics.ToJson());

            return Ok(referraLinksStatistics);
        }

        /// <summary>
        /// Request money transfer referral link - reserved for version 2   
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        //[HttpPost("giftCoinLinks")] - for v2
        //[SwaggerOperation("RequestGiftCoinsReferralLink")]
        //[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
        //[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.NotFound)]
        //[ProducesResponseType(typeof(RequestRefLinkResponse), (int)HttpStatusCode.Created)]
        private async Task<IActionResult> RequestGiftCoinsReferralLink([FromBody] GiftCoinsReferralLinkRequest request)
        {
            if (request == null)
            {
                return await LogAndReturnBadRequest("", ControllerContext, Phrases.InvalidRequest);
            }

            if (request.Amount <= 0)
            {
                return await LogAndReturnBadRequest(request, ControllerContext, Phrases.InvalidAmount);
            }

            if (String.IsNullOrEmpty(request.SenderClientId))
            {
                return await LogAndReturnBadRequest(request, ControllerContext, Phrases.InvalidSenderClientId);
            }

            var asset = (await _assets.GetDictionaryAsync()).Values.FirstOrDefault(v => v.Id == request.Asset);

            if (String.IsNullOrEmpty(request.Asset) || asset == null)
            {
                return await LogAndReturnBadRequest(request, ControllerContext, Phrases.InvalidAsset);
            }

            var clientBalances = await _balancesClient.GetClientBalances(request.SenderClientId);

            if (clientBalances == null)
            {
                return await LogAndReturnNotFound(request, ControllerContext, $"Cant get clientBalance of asset {asset.Name} for client id {request.SenderClientId}.");
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

        [HttpPost("invitation")]
        [SwaggerOperation("RequestInvitationReferralLink")]
        [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(RequestRefLinkResponse), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(RequestRefLinkResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> RequestInvitationReferralLink([FromBody] InvitationReferralLinkRequest request)
        {
            if (request == null)
            {
                return await LogAndReturnBadRequest("", ControllerContext, Phrases.InvalidRequest);
            }

            if (String.IsNullOrEmpty(request.SenderClientId))
            {
                return await LogAndReturnBadRequest(request, ControllerContext, Phrases.InvalidSenderClientId);
            }


            var invitation = _referralLinksService.GetInvitationLinksForSenderId(request.SenderClientId).FirstOrDefault();
            if (invitation != null)
            {
                await LogInfo(request, ControllerContext, $"Invitation link requested by SenderClientId {request.SenderClientId}. RefLinkId {invitation.Id}");
                return Ok(new RequestRefLinkResponse { RefLinkUrl = invitation.Url, RefLinkId = invitation.Id });
            }

            var referralLink = await _referralLinksService.CreateInvitationLink(request);

            await LogInfo(request, ControllerContext, referralLink.ToJson());

            return Created(uri: $"api/referralLinks/{referralLink.Id}", value: new RequestRefLinkResponse { RefLinkUrl = referralLink.Url, RefLinkId = referralLink.Id });
        }

        private string ValidateClaimRefLinkAndRequest(IReferralLink refLink, ClaimReferralLinkRequest request, string refLinkId)
        {
            if (request.RecipientClientId == null)
            {
                return "RecipientClientId not found or not supplied";
            }
            if (refLinkId == null && request.ReferalLinkUrl == null)
            {
                return "ReferalLinkId and ReferalLinkUrl not supplied. Please specify either of them.";
            }

            if (refLink == null)
            {
                return "RefLink not found by id or url.";
            }

            if (refLink.SenderClientId == request.RecipientClientId)
            {
                return "RecipientClientId can't be the same as SenderClientId. Client cant claim their own ref link.";
            }

            if (Math.Abs(refLink.Amount) < MinimalAmount && refLink.Type == ReferralLinkType.GiftCoins.ToString())
            {
                return $"Requested amount for RefLink with id {refLink.Id} is 0 (not set). 0 amount gift links are not allowed.";
            }

            if (refLink.Type == ReferralLinkType.GiftCoins.ToString() && refLink.State != ReferralLinkState.SentToLykkeSharedWallet.ToString())
            {
                return $"RefLink type {refLink.Type} with state {refLink.State} can not be claimed.";
            }

            if (refLink.Type == ReferralLinkType.Invitation.ToString() && refLink.State != ReferralLinkState.Created.ToString())
            {
                return $"RefLink type {refLink.Type} with state {refLink.State} can not be claimed.";
            }

            if (refLink.ExpirationDate.HasValue && refLink.ExpirationDate.Value.CompareTo(DateTime.UtcNow) < 0)
            {
                return $"RefLink is expired at {refLink.ExpirationDate.Value} and can not be claimed.";
            }

            return null;
        }

        //[HttpPut("giftCoinLinks/{refLinkId}/claim")]  - reserved for version 2
        //[SwaggerOperation("ClaimGiftCoins")]
        //[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
        //[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.NotFound)]
        //[ProducesResponseType(typeof(ClaimRefLinkResponse), (int)HttpStatusCode.OK)]
        private async Task<IActionResult> ClaimGiftCoins([FromBody] ClaimReferralLinkRequest request, string refLinkId)
        {
            var refLink = await _referralLinksService.GetReferralLinkById(refLinkId ?? "");

            var validationError = ValidateClaimRefLinkAndRequest(refLink, request, refLinkId);
            if (!String.IsNullOrEmpty(validationError))
            {
                return await LogAndReturnBadRequest(request, ControllerContext, validationError);
            }

            var asset = (await _assets.GetDictionaryAsync()).Values.FirstOrDefault(v => v.Id == refLink.Asset);

            if (asset == null)
            {
                return await LogAndReturnBadRequest(request, ControllerContext, $"Asset with id {refLink.Asset} for Referral link {refLink.Id} not found. Check transfer's history.");
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

                    return Ok(new ClaimRefLinkResponse { TransactionRewardRecipient = transactionRewardRecipient.TransactionId, SenderOffchainTransferId = refLink.SenderOffchainTransferId });
                }
                return await LogAndReturnNotFound(request, ControllerContext, $"TransactionRewardRecipientError: Code: {transactionRewardRecipient.Code}, Message: {transactionRewardRecipient.Message}");
            }
            catch (TransactionAbortedException ex)
            {
                await LogError(new { Request = request, RefLink = refLink ?? new ReferralLink() }, ControllerContext, ex);
                return NotFound(ex.Message);
            }
            catch (ApplicationException ex)
            {
                await LogError(new { Request = request, RefLink = refLink ?? new ReferralLink() }, ControllerContext, ex);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                await LogError(new { Request = request, RefLink = refLink ?? new ReferralLink() }, ControllerContext, ex);
                return NotFound(ex.Message);
            }

        }

        /// <summary>
        /// Claim invitation referral link.
        /// </summary>
        /// <param name="refLinkId"></param>
        /// <param name="request"></param>
        /// <returns></returns>

        [HttpPut("invitation/{refLinkId}/claim")]
        [SwaggerOperation("ClaimInvitationLink")]
        [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(ClaimRefLinkResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> ClaimInvitationLink(string refLinkId, [FromBody] ClaimReferralLinkRequest request )
        {
            if (!request.IsNewClient)
            {
                return await LogAndReturnBadRequest(request, ControllerContext, "Not a new client.");
            }

            var refLink = await _referralLinksService.GetReferralLinkById(refLinkId ?? "") ?? await _referralLinksService.GetReferralLinkByUrl(request.ReferalLinkUrl ?? "");

            var validationError = ValidateClaimRefLinkAndRequest(refLink, request, refLinkId);
            if (!String.IsNullOrEmpty(validationError))
            {
                return await LogAndReturnBadRequest(request, ControllerContext, validationError);
            }

            var claims = (await _referralLinkClaimsService.GetRefLinkClaims(refLink.Id)).ToList();

            var alreadyClaimedByThisRecipient = claims.Any(c => c.RecipientClientId == request.RecipientClientId);
            if (alreadyClaimedByThisRecipient)
            {
                return await LogAndReturnBadRequest(request, ControllerContext, $"Link already claimed by client id {request.RecipientClientId}");
            }

            bool shouldReceiveReward = await ShoulReceiveReward(claims, refLink);

            try
            {
                var newRefLinkClaimRecipient = await CreateNewRefLinkClaim(refLink, request.RecipientClientId, shouldReceiveReward, true);

                if (shouldReceiveReward)
                {
                    var newRefLinkClaimSender = await CreateNewRefLinkClaim(refLink, refLink.SenderClientId, true, false);

                    if (Math.Abs(refLink.Amount) < MinimalAmount)
                    {
                        return Ok(new ClaimRefLinkResponse());
                    }

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
                    return await LogAndReturnInternalServerError(request, ControllerContext, $"TransactionRewardRecipientError: Code: {transactionRewardRecipient.Code}, {transactionRewardRecipient.Message}; TransactionRewardSenderError: Code: {transactionRewardSender.Code}, {transactionRewardSender.Message}");
                }

                return Ok(new ClaimRefLinkResponse());
            }
            catch (Exception ex)
            {
                return await LogAndReturnInternalServerError(request, ControllerContext, $"{ex.ToJson()}.");
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
            var countOfNewClientClaims = claims.Count(c => c.IsNewClient && c.ShouldReceiveReward && c.RecipientClientId != refLink.SenderClientId);
            bool shouldReceiveReaward = countOfNewClientClaims < _settings.InvitationLinkSettings.MaxNumOfClientsToReceiveReward;

            if (!shouldReceiveReaward)
            {
                await LogInfo(refLink, ControllerContext, "MaxNumOfClientsToReceiveReward reached. Recipient and sender wont get reward coins.");
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

