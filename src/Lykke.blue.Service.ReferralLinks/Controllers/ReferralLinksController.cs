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
using Lykke.blue.Service.ReferralLinks.Models.GiftCoinRequests;
using Lykke.blue.Service.ReferralLinks.Models.RefLinkResponseModels;
using Lykke.blue.Service.ReferralLinks.Modules.Validation;
using Lykke.blue.Service.ReferralLinks.Responses;
using Lykke.blue.Service.ReferralLinks.Services.Domain;
using Lykke.blue.Service.ReferralLinks.Services.ExchangeOperations;
using Lykke.blue.Service.ReferralLinks.Strings;
using Lykke.Service.ExchangeOperations.Client;
using Lykke.Service.ExchangeOperations.Client.AutorestClient.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

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
        
        private readonly ReferralLinksSettings _settings;
        private readonly double MinimalAmount = 0.00000001;

        public ReferralLinksController(
            ILog log,
            IReferralLinksService referralLinksService,
            IStatisticsService statisticsService,
            CachedDataDictionary<string, Lykke.Service.Assets.Client.Models.Asset> assets,
            ISrvKycForAsset srvKycForAsset,
            ReferralLinksSettings settings,
            IReferralLinkClaimsService referralLinkClaimsService,
            ExchangeService exchangeService) : base(log)
        {

            _referralLinksService = referralLinksService;
            _assets = assets;
            _srvKycForAsset = srvKycForAsset;
            _exchangeService = exchangeService;
            _settings = settings;
            _referralLinkClaimsService = referralLinkClaimsService;
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
        /// Get mass generated referral link by id.
        /// </summary>
        /// <param name="senderId">The Id of the referral link group</param>
        /// <returns></returns>
        [HttpGet("giftCoins/sender/{senderId}")]
        [SwaggerOperation("GetGroupReferralLinkBySenderId")]
        [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(GetReferralLinkResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetGroupReferralLinkBySenderId(string senderId)
        {
            if (String.IsNullOrEmpty(senderId))
            {
                return await LogAndReturnNotFound(senderId, ControllerContext, "Requested id cant be empty");
            }

            var groupOfReferralLinks = (await _referralLinksService.GetGiftCoinLinksBySenderId(senderId)).ToList();

            if (!groupOfReferralLinks.Any())
            {
                var msg = $"No gift coin links found for sender id {senderId}.";
                return await LogAndReturnNotFound(senderId, ControllerContext, msg);
            }

            var result = Mapper.Map<IEnumerable<IReferralLink>, IEnumerable<GetReferralLinkResponse>>(groupOfReferralLinks);
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
            var decoded = WebUtility.UrlDecode(url ?? "");

            if (String.IsNullOrEmpty(decoded))
            {
                return await LogAndReturnBadRequest(url, ControllerContext, "Requested url cant be empty"); 
            }

            var referralLink = await _referralLinksService.GetReferralLinkByUrl(decoded);

            if (referralLink == null)
            {
                return await LogAndReturnNotFound(decoded, ControllerContext, $"Ref link with url {decoded} does not exist");
            }

            var result = Mapper.Map<GetReferralLinkResponse>(referralLink);

            return Ok(result);
        }

        /// <summary>
        /// Get referral links statistics by sender client id.
        /// </summary>
        /// <param name="senderClientId">Sender client id by which we want to get statistics.</param>
        /// <returns></returns>
        [HttpGet("statistics")]
        [SwaggerOperation("GetReferralLinksStatisticsBySenderId")]
        [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(GetReferralLinksStatisticsBySenderIdResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetReferralLinksStatisticsBySenderId(string senderClientId)
        {
            if (String.IsNullOrEmpty(senderClientId))
            {
                return await LogAndReturnBadRequest(senderClientId, ControllerContext, Phrases.InvalidSenderClientId);
            }

            var referraLinksStatistics = await _statisticsService.GetStatistics(senderClientId);

            await LogInfo(senderClientId, ControllerContext, referraLinksStatistics.ToJson());

            return Ok(referraLinksStatistics);
        }

        /// <summary>
        /// Generate Gift Coins referral link
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("giftCoins/group")]
        [SwaggerOperation("RequestGiftCoinsReferralLink")]
        [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(RequestRefLinkResponse), (int)HttpStatusCode.Created)]
        public async Task<IActionResult> GroupGenerateGiftCoinLinks([FromBody] GiftCoinRequestGroup request)
        {
            var validationError = await ValidateGiftCoinsLinkRequest(request);
            if (!String.IsNullOrEmpty(validationError))
            {
                return await LogAndReturnBadRequest(request, ControllerContext, validationError);
            }

            var refLinkGroup =  await _referralLinksService.CreateGroupOfGiftCoinLinks(request.SenderClientId, request.Asset, request.AmountForEachLink);

            var groupTransferResult =  await _exchangeService.TransferToSharedWallet(request.SenderClientId, refLinkGroup.Sum(r => r.Amount), request.Asset);

            if (groupTransferResult.IsOk())
            {
                return Created($"api/referralLinks/giftCoins/sender/{request.SenderClientId}", "");
            }
            return await LogAndReturnInternalServerError(request, ControllerContext, groupTransferResult.Message);
        }

        private async Task<string> ValidateGiftCoinsLinkRequest<T>(T request) where T : GiftCoinRequestBase
        {
            if (request == null)
            {
                return Phrases.InvalidRequest;
            }

            if (String.IsNullOrEmpty(request.SenderClientId))
            {
                return Phrases.InvalidSenderClientId;
            }

            var asset = (await _assets.GetDictionaryAsync()).Values.FirstOrDefault(v => v.Id == request.Asset);

            if (asset == null)
            {
                return Phrases.InvalidAsset;
            }

            if (await _srvKycForAsset.IsKycNeeded(request.SenderClientId, asset.Id))
            {
                return $"KYC needed for sender client id {request.SenderClientId} before sending asset with id {asset.Id}";
            }

            var refLinkTotalAmount = 0.0;

            var singleRefLinkRequest = request as GiftCoinRequest;

            if (singleRefLinkRequest!=null)
            {
                refLinkTotalAmount = singleRefLinkRequest.Amount;
            }

            var groupRefLinkRequest = request as GiftCoinRequestGroup;

            if (groupRefLinkRequest != null)
            {
                if (groupRefLinkRequest.AmountForEachLink.Length == 0 || groupRefLinkRequest.AmountForEachLink.Sum() <= 0 || groupRefLinkRequest.AmountForEachLink.Any(linkAmount => linkAmount <= 0))
                {
                    return Phrases.InvalidAmount;
                }

                refLinkTotalAmount = groupRefLinkRequest.AmountForEachLink.Sum();
            }
            
            if (refLinkTotalAmount <=0 || !await _referralLinksService.HasEnoughBalance(request.SenderClientId, asset.Id, refLinkTotalAmount))
            {
                return Phrases.InvalidBalanceAmount;
            }

            return "";
        }



        /// <summary>
        /// Generate Gift Coins referral link
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("giftCoins")]
        [SwaggerOperation("RequestGiftCoinsReferralLink")]
        [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(RequestRefLinkResponse), (int)HttpStatusCode.Created)]
        public async Task<IActionResult> RequestGiftCoinsReferralLink([FromBody] GiftCoinRequest request)
        {
            var validationError = await ValidateGiftCoinsLinkRequest(request);
            if (!String.IsNullOrEmpty(validationError))
            {
                return await LogAndReturnBadRequest(request, ControllerContext, validationError);
            }

            var referralLink = await _referralLinksService.CreateGiftCoinLink(request.SenderClientId, request.Asset, request.Amount);

            var transferResult = await _exchangeService.TransferToSharedWallet(request.SenderClientId, referralLink.Amount, request.Asset);

            if (transferResult.IsOk())
            {
                await LogInfo(request, ControllerContext, referralLink.ToJson());

                return Created(uri: $"api/referralLinks/{referralLink.Id}", value: new RequestRefLinkResponse { RefLinkId = referralLink.Id, RefLinkUrl = referralLink.Url });
            }

            return await LogAndReturnInternalServerError(request, ControllerContext, transferResult.Message);
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

        private async Task<string>  ValidateClaimRefLinkAndRequest(IReferralLink refLink, ClaimReferralLinkRequest request, string refLinkId)
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

            var asset = (await _assets.GetDictionaryAsync()).Values.FirstOrDefault(v => v.Id == refLink.Asset);

            if (asset == null)
            {
                return Phrases.InvalidAsset;
            }

            if (refLink.SenderClientId == request.RecipientClientId)
            {
                return "RecipientClientId can't be the same as SenderClientId. Client cant claim their own ref link.";
            }

            if (refLink.Type == ReferralLinkType.GiftCoins.ToString())
            {
                if(refLink.State != ReferralLinkState.SentToLykkeSharedWallet.ToString()) return $"RefLink type {refLink.Type} with state {refLink.State} can not be claimed.";

                if(Math.Abs(refLink.Amount) < MinimalAmount) return $"Requested amount for RefLink with id {refLink.Id} is 0 (not set). 0 amount gift links are not allowed.";

                if (await _srvKycForAsset.IsKycNeeded(request.RecipientClientId, refLink.Asset))
                    return $"KYC needed for recipient client id {request.RecipientClientId} before claiming asset {refLink.Asset}";
            }

            if(refLink.Type == ReferralLinkType.Invitation.ToString())
            {
                if(!request.IsNewClient) return "Not a new client.";

                if ((await _referralLinkClaimsService.GetRefLinkClaimsForClient(refLink.Id, request.RecipientClientId)).Any()) return $"Link already claimed by client id {request.RecipientClientId}";
            }

            if (refLink.ExpirationDate.HasValue && refLink.ExpirationDate.Value.CompareTo(DateTime.UtcNow) < 0)
            {
                return $"RefLink is expired at {refLink.ExpirationDate.Value} and can not be claimed.";
            }

            return null;
        }


        /// <summary>
        /// Claim gift coins referral link
        /// </summary>
        /// <param name="refLinkId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("giftCoins/{refLinkId}/claim")]
        [SwaggerOperation("ClaimGiftCoins")]
        [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(ClaimRefLinkResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> ClaimGiftCoins(string refLinkId, [FromBody] ClaimReferralLinkRequest request)
        {
            var refLink = await _referralLinksService.Get(refLinkId ?? "");

            var validationError = await ValidateClaimRefLinkAndRequest(refLink, request, refLinkId);

            if (!String.IsNullOrEmpty(validationError))
            {
                return await LogAndReturnBadRequest(request, ControllerContext, validationError);
            }
            
            await SetRefLinkStateToClaimed(refLink); 

            var newRefLinkClaimRecipient = await CreateNewRefLinkClaim(refLink, request.RecipientClientId, true, request.IsNewClient);

            try
            {
                var transactionRewardRecipient = await _exchangeService.TransferFromSharedWallet(refLink, request.RecipientClientId, ControllerContext.GetControllerAndAction());

                if (transactionRewardRecipient.IsOk())
                {
                    await SetRefLinkClaimTransactionId(transactionRewardRecipient, newRefLinkClaimRecipient);
                    return Ok(new ClaimRefLinkResponse { TransactionRewardRecipient = transactionRewardRecipient.TransactionId, SenderOffchainTransferId = refLink.SenderOffchainTransferId });
                }
                return await LogAndReturnInternalServerError(request, ControllerContext, $"TransactionRewardRecipientError: Code: {transactionRewardRecipient.Code}, Message: {transactionRewardRecipient.Message}");
            }
            catch (Exception ex)
            {
                return await LogAndReturnInternalServerError(new { Request = request, RefLinkId = refLinkId }.ToJson(), ControllerContext, ex);
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
            var refLink = await _referralLinksService.Get(refLinkId ?? "") ?? await _referralLinksService.GetReferralLinkByUrl(request.ReferalLinkUrl ?? "");

            var validationError = await ValidateClaimRefLinkAndRequest(refLink, request, refLinkId);

            if (!String.IsNullOrEmpty(validationError))
            {
                return await LogAndReturnBadRequest(request, ControllerContext, validationError);
            }
            
            try
            {
                await IncrementClaimsCountAsync(refLink);

                bool shouldReceiveReward = await ShoulReceiveReward(refLink);

                var newRefLinkClaimRecipient = await CreateNewRefLinkClaim(refLink, request.RecipientClientId, shouldReceiveReward, true);

                if (shouldReceiveReward)
                {
                    var newRefLinkClaimSender = await CreateNewRefLinkClaim(refLink, refLink.SenderClientId, true, false);

                    if (Math.Abs(refLink.Amount) < MinimalAmount)
                    {
                        await LogWarn(request, ControllerContext, $"Invitation link with id {refLink.Id} is claimed, but the link is set to reward amount 0. If this is intentional, please act accordingly - possibly rewarding the recipients manually. Records for the claim will be created in DB, but with empty transaction ID. Invitation links get created with 0 amount if the relevant setting in config is set to 0. ");
                        return Ok(new ClaimRefLinkResponse());
                    }

                    var txSender = await _exchangeService.TransferFromSharedWallet(refLink, refLink.SenderClientId, ControllerContext.GetControllerAndAction());
                    await SetRefLinkClaimTransactionId(txSender, newRefLinkClaimSender);

                    var txRecipient = await _exchangeService.TransferFromSharedWallet(refLink, request.RecipientClientId, ControllerContext.GetControllerAndAction());
                    await SetRefLinkClaimTransactionId(txRecipient, newRefLinkClaimRecipient);

                    if (txSender.IsOk() && txRecipient.IsOk())
                    {
                        return Ok
                            (
                                new ClaimRefLinkResponse
                                {
                                    TransactionRewardSender = txSender.TransactionId,
                                    TransactionRewardRecipient = txRecipient.TransactionId
                                }
                            );
                    }
                    return await LogAndReturnInternalServerError(request, ControllerContext, $"TransactionRecipient: Code: {txRecipient.Code}, {txRecipient.Message}, recipient TxId: {txRecipient.TransactionId}; TransactionSender: Code: {txSender.Code}, {txSender.Message}, sender TxId: {txSender.TransactionId}");
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

        private async Task<bool> ShoulReceiveReward(IReferralLink refLink)
        {
            bool shouldReceiveReaward = refLink.ClaimsCount <= _settings.InvitationLinkSettings.MaxNumOfClientsToReceiveReward;

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
        
        private async Task SetRefLinkStateToClaimed(IReferralLink refLink)
        {
            refLink.State = ReferralLinkState.Claimed.ToString();
            refLink.ClaimsCount += 1;
            await _referralLinksService.UpdateAsyncWithETagCheck(refLink);
            await LogInfo(refLink, ControllerContext, $"RefLink {refLink.Id} state set to claimed and ClaimsCount increased to {refLink.ClaimsCount}");
        }

        private async Task IncrementClaimsCountAsync(IReferralLink refLink)
        {
            refLink.ClaimsCount += 1;
            await _referralLinksService.UpdateAsyncWithETagCheck(refLink);
            await LogInfo(refLink, ControllerContext, $"RefLink {refLink.Id} ClaimsCount increased to {refLink.ClaimsCount}");
        }


    }
}

