using AutoMapper;
using Common.Log;
using Lykke.Service.Assets.Client;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ReferralLinks.Core.Domain.ReferralLink;
using Lykke.Service.ReferralLinks.Core.Services;
using Lykke.Service.ReferralLinks.Core.Settings;
using Lykke.Service.ReferralLinks.Requests;
using Lykke.Service.ReferralLinks.Responses;
using Lykke.SettingsReader;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.SwaggerGen.Annotations;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Lykke.Service.ReferralLinks.Strings;
using Lykke.Service.Balances.Client;
using Lykke.Service.ReferralLinks.AzureRepositories.ReferralLink;
using Lykke.Service.ReferralLinks.Models;
using Common;
using Lykke.Service.ReferralLinks.Core.BitCoinApi.Models;
using Lykke.Service.ReferralLinks.Core.Domain.Offchain;
using Lykke.Service.ReferralLinks.Core.Kyc;
using Lykke.Service.ExchangeOperations.Client;
using Lykke.Service.ReferralLinks.Core.Settings.ServiceSettings;
using Core.BitCoin.BitcoinApi.Models;
using Lykke.Service.ReferralLinks.Services.Domain;
using System.Transactions;
using Lykke.Service.ReferralLinks.Modules.Validation;
using System.Linq;
using Lykke.Service.ExchangeOperations.Client.AutorestClient.Models;
using Lykke.Service.ReferralLinks.Extensions;
using Lykke.Service.ReferralLinks.Core.Domain.Requests;

namespace Lykke.Service.ReferralLinks.Controllers
{
    [Route("api/referralLinks")]
    [ValidateModel]
    public class ReferralLinksController : Controller
    {
        private readonly ILog _log;
        private readonly IReferralLinksService _referralLinksService;
        private readonly IReferralLinkClaimsService _referralLinkClaimsService;        
        private readonly IClientAccountClient _clientAccountClient;
        //private readonly IAssetsService _assetsClient;
        private readonly ISrvKycForAsset _srvKycForAsset;
        private readonly IExchangeOperationsServiceClient _exchangeOperationsService;
        private readonly CachedDataDictionary<string, Lykke.Service.Assets.Client.Models.Asset> _assets;
        private readonly IBalancesClient _balancesClient;
        private readonly ReferralLinksSettings _settings;

        public ReferralLinksController(
            ILog log,
            IReferralLinksService referralLinksService,
            IClientAccountClient clientAccountClient,
            //IAssetsService assetsClient,
            CachedDataDictionary<string, Lykke.Service.Assets.Client.Models.Asset> assets,
            ISrvKycForAsset srvKycForAsset,
            IExchangeOperationsServiceClient exchangeOperationsService,
            ReferralLinksSettings settings,
            IReferralLinkClaimsService referralLinkClaimsService,
            IBalancesClient balancesClient)
        {

            _log = log;
            _referralLinksService = referralLinksService;
            _clientAccountClient = clientAccountClient;
            //_assetsClient = assetsClient ?? throw new ArgumentException(nameof(assetsClient));
            _assets = assets;
            _srvKycForAsset = srvKycForAsset;
            _exchangeOperationsService = exchangeOperationsService;
            _settings = settings;
            _referralLinkClaimsService = referralLinkClaimsService;
            _balancesClient = balancesClient;
        }

        ///// <summary>
        ///// Create referral link.
        ///// </summary>
        ///// <param name="request"></param>
        ///// <returns></returns>
        //[HttpPost]
        //[SwaggerOperation("CreateReferralLink")]
        //[ProducesResponseType((int)HttpStatusCode.BadRequest)]
        //[ProducesResponseType(typeof(CreateReferralLinkResponse), (int)HttpStatusCode.Created)]
        //public async Task<IActionResult> Create([FromBody] CreateReferralLinkRequest request)
        //{
        //    if (request == null)
        //    {
        //        return BadRequest(Phrases.InvalidRequest);
        //    }

        //    if (request.Amount <= 0)
        //    {
        //        return BadRequest(Phrases.InvalidAmount);
        //    }

        //    if (String.IsNullOrEmpty(request.SenderClientId) || await _clientAccountClient.GetClientById(request.SenderClientId) == null)
        //    {
        //        return BadRequest(Phrases.InvalidSenderClientId);
        //    }

        //    var asset = (await _assets.GetDictionaryAsync()).Values.Where(v => v.Symbol == request.Asset).FirstOrDefault();

        //    if (String.IsNullOrEmpty(request.Asset) || asset == null)
        //    {
        //        return BadRequest(Phrases.InvalidAsset);
        //    }

        //    var referralLinksLimitReached = await _referralLinksService.IsInvitationLinksMaxNumberReachedForSender(request.ClaimingClientId);

        //    if (referralLinksLimitReached)
        //    {
        //        return BadRequest(Phrases.ReferralLinksLimitReached);
        //    }

        //    var clientBalances = await _balancesClient.GetClientBalances(request.SenderClientId);
        //    var balance = clientBalances.FirstOrDefault(x => x.AssetId == asset.Id)?.Balance;

        //    if (!balance.HasValue ||  ((decimal)(balance.Value))  < request.Amount)
        //    {
        //        return BadRequest(Phrases.InvalidTreesAmount);
        //    }

        //    var referralLink = Mapper.Map<CreateReferralLinkResponse>(await _referralLinksService.Create(request));

        //    return Created(uri: $"api/referralLinks/{referralLink.Id}", value: referralLink);
        //}


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
        /// Get referral links by sender client id or state.
        /// </summary>
        /// <param name="senderClientId">Sender client id for which we wanna find referral links.</param>
        /// <param name="state">State by which we wanna to find referral links.</param>
        /// <returns></returns>
        /// REMARK: Swagger specification DOES NOT support optional parameters in path.
        [HttpGet]
        [SwaggerOperation("GetReferralLinksBySenderIdAndOrStatus")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(IEnumerable<GetReferralLinkResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetReferralLinksBySenderIdAndOrStatus([FromQuery] string senderClientId, [FromQuery] string state)
        {
            if (String.IsNullOrEmpty(senderClientId) && String.IsNullOrEmpty(state))
            {
                return BadRequest(Phrases.InvalidRequest);
            }

            if (String.IsNullOrEmpty(senderClientId) && !String.IsNullOrEmpty(state) && !Enum.IsDefined(typeof(ReferralLinkState), state))
            {
                return BadRequest(Phrases.InvalidState);
            }

            if (String.IsNullOrEmpty(state) && !String.IsNullOrEmpty(senderClientId) && await _clientAccountClient.GetClientById(senderClientId) == null)
            {
                return BadRequest(Phrases.InvalidSenderClientId);
            }

            if (!String.IsNullOrEmpty(senderClientId) && !String.IsNullOrEmpty(state)
                && (!Enum.IsDefined(typeof(ReferralLinkState), state) || await _clientAccountClient.GetClientById(senderClientId) == null))
            {
                return BadRequest(Phrases.InvalidRequest);
            }

            ReferralLinkState stateParsed;
            if(!Enum.TryParse<ReferralLinkState>(state, out stateParsed))
            {
                return BadRequest(Phrases.InvalidState);
            }


            var referralLinks = await _referralLinksService.GetReferralLinksBySenderClientIdAndOrStatus(senderClientId, stateParsed);

            var result = Mapper.Map<IEnumerable<GetReferralLinkResponse>>(referralLinks);

            return Ok(result);
        }

        /// <summary>
        /// Change referral link state.
        /// </summary>
        /// <param name="id">Id of a referral link we wanna change state for.</param>
        /// <param name="state">New referral link state.</param>
        /// <returns></returns>
        [HttpPut("updateState/{id}/{state}")]
        [SwaggerOperation("UpdateReferralLinkState")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> UpdateState(string id, string state)
        {
            if (String.IsNullOrEmpty(id)
                || String.IsNullOrEmpty(state)
                || !Enum.IsDefined(typeof(ReferralLinkState), state))
            {
                return BadRequest(Phrases.InvalidRequest);
            }

            var referralLink = await _referralLinksService.Get(id);

            if (referralLink == null)
            {
                return BadRequest(Phrases.InvalidReferralLinkId);
            }

            await _referralLinksService.UpdateState(id, Enum.Parse<ReferralLinkState>(state));

            return NoContent();
        }

        /// <summary>
        /// Get referral links statistics by sender client id.
        /// </summary>
        /// <param name="senderClientId">Sender client id by which we wanna get statistics.</param>
        /// <returns></returns>
        [HttpGet("getReferralLinksStatistics/{senderClientId}")]
        [SwaggerOperation("GetReferralLinksStatisticsBySenderId")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(GetReferralLinksStatisticsBySenderIdResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetReferralLinksStatisticsBySenderId(string senderClientId)
        {
            if (String.IsNullOrEmpty(senderClientId) || await _clientAccountClient.GetClientById(senderClientId) == null)
            {
                return BadRequest(Phrases.InvalidSenderClientId);
            }

            var referraLinksStatistics = await _referralLinksService.GetReferralLinksStatisticsBySenderId(senderClientId);

            return Ok(referraLinksStatistics);
        }

        /// <summary>
        /// Set referral link Url.
        /// </summary>
        /// <param name="id">Id of a referral link we wanna update url for.</param>
        /// <param name="url">Url that we wanna set.</param>
        /// <returns></returns>
        [HttpPut("setUrl/{id}/{url}")]
        [SwaggerOperation("SetReferralLinkUrl")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> SetUrl(string id, string url)
        {
            if (String.IsNullOrEmpty(id) || String.IsNullOrEmpty(url))
            {
                return BadRequest(Phrases.InvalidRequest);
            }

            var referralLink = await _referralLinksService.Get(id);

            if (referralLink == null)
            {
                return BadRequest(Phrases.InvalidReferralLinkId);
            }

            await _referralLinksService.SetUrl(id, url);

            return NoContent();
        }

        ///// <summary>
        ///// Claim gift coins.
        ///// </summary>
        ///// <param name="request"></param>
        ///// <returns></returns>
        //[HttpPut("claimGiftCoins")]
        //[SwaggerOperation("ClaimGiftCoins")]
        //[ProducesResponseType((int)HttpStatusCode.BadRequest)]
        //[ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        //public async Task<IActionResult> ClaimGiftCoins([FromBody] ClaimGiftCoinsRequest request)
        //{
        //    if(request == null)
        //    {
        //        return BadRequest(Phrases.InvalidRequest);
        //    }

        //    if (String.IsNullOrEmpty(request.Id) || await _referralLinksService.Get(request.Id) == null)
        //    {
        //        return BadRequest(Phrases.InvalidReferralLinkId);
        //    }

        //    if (String.IsNullOrEmpty(request.ClaimingUserId) || await _clientAccountClient.GetClientById(request.ClaimingUserId) == null)
        //    {
        //        return BadRequest(Phrases.InvalidClaimingClientId);
        //    }

        //    var state = await _referralLinksService.ClaimGiftCoins(request.Id, request.IsNewUser, request.ClaimingUserId);

        //    return Ok(state);
        //}

        /// <summary>
        /// Request money transfer referral link.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("requestMoneyTransferReferralLink")]
        [SwaggerOperation("RequestMoneyTransferReferralLink")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> RequestMoneyTransferReferralLink([FromBody] MoneyTransferReferralLinkRequest request)
        {
            if (request == null)
            {
                return BadRequest(Phrases.InvalidRequest);
            }

            if(request.Amount <= 0)
            {
                return BadRequest(Phrases.InvalidAmount);
            }

            if (String.IsNullOrEmpty(request.SenderClientId) || await _clientAccountClient.GetClientById(request.SenderClientId) == null)
            {
                return BadRequest(Phrases.InvalidSenderClientId);
            }

            var asset = (await _assets.GetDictionaryAsync()).Values.Where(v => v.Symbol == request.Asset).FirstOrDefault();

            if (String.IsNullOrEmpty(request.Asset) || asset == null)
            {
                return BadRequest(Phrases.InvalidAsset);
            }

            var clientBalances = await _balancesClient.GetClientBalances(request.SenderClientId);
            var balance = clientBalances.FirstOrDefault(x => x.AssetId == asset.Id)?.Balance;

            if(!balance.HasValue || ((decimal) balance.Value) < request.Amount)
            {
                return BadRequest(Phrases.InvalidTreesAmount);
            }

            var referralLink = Mapper.Map<CreateReferralLinkResponse>(await _referralLinksService.CreateMoneyTransferLink(request));

            return Created(uri: $"api/referralLinks/{referralLink.Id}", value: referralLink.Id);
        }
        

        /// <summary>
        /// Request invitation referral link.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("requestInvitationReferralLink")]
        [SwaggerOperation("RequestInvitationReferralLink")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> RequestInvitationReferralLink([FromBody] InvitationReferralLinkRequest request)
        {
            if (request == null)
            {
                return BadRequest(Phrases.InvalidRequest);
            }

            //if (request.Amount <= 0)
            //{
            //    return BadRequest(Phrases.InvalidAmount);
            //}

            if (String.IsNullOrEmpty(request.SenderClientId) || await _clientAccountClient.GetClientById(request.SenderClientId) == null)
            {
                return BadRequest(Phrases.InvalidSenderClientId);
            }

            //var asset = (await _assets.GetDictionaryAsync()).Values.Where(v => v.Symbol == request.Asset).FirstOrDefault();

            //if (String.IsNullOrEmpty(request.Asset) || asset == null)
            //{
            //    return BadRequest(Phrases.InvalidAsset);
            //}

            var referralLinksLimitReached = await _referralLinksService.IsInvitationLinksMaxNumberReachedForSender(request.SenderClientId);

            if (referralLinksLimitReached)
            {
                return BadRequest(Phrases.ReferralLinksLimitReached);
            }

            var referralLink = Mapper.Map<CreateReferralLinkResponse>(await _referralLinksService.CreateInvitationLink(request));

            return Created(uri: $"api/referralLinks/{referralLink.Id}", value: referralLink.Url);
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

            if (refLink.Type == ReferralLinkType.MoneyTransfer && refLink.State != ReferralLinkState.SentToLykkeSharedWallet)
            {
                return $"RefLink type {refLink.Type} with state {refLink.State} can not be claimed.";
            }

            if (refLink.Type == ReferralLinkType.Invitation && refLink.State != ReferralLinkState.Created)
            {
                return $"RefLink type {refLink.Type} with state {refLink.State} can not be claimed.";
            }

            if (refLink.ExpirationDate.HasValue && refLink.ExpirationDate.Value.CompareTo(DateTime.Now) > 0)
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
                return BadRequest(validationError);
            }

            var asset = (await _assets.GetDictionaryAsync()).Values.Where(v => v.Symbol == refLink.Asset).FirstOrDefault();

            if (asset == null)
            {
                return BadRequest($"Asset {refLink.Asset} for Referral link {refLink.Id} not found. Check transfer's history.");
            }
            
            if (await _srvKycForAsset.IsKycNeeded(request.RecipientClientId, asset.Id))
                return BadRequest("Kyc for recipient Needed");

            var newRefLinkClaimRecipient = await CreateNewRefLinkClaim(refLink, request.RecipientClientId, true, request.IsNewClient);

            var transactionRewardRecipient = await TransferRewardCoins(refLink, request, request.RecipientClientId);

            try
            {
                if (transactionRewardRecipient.IsOk())
                {
                    using (TransactionScope scope = new TransactionScope())
                    {
                        await SetRefLinkClaimTransactionId(transactionRewardRecipient, newRefLinkClaimRecipient);
                        refLink.State = ReferralLinkState.Claimed;
                        await _referralLinksService.UpdateAsync(refLink);
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
                await LogError(refLink, request, request.RecipientClientId, nameof(ClaimGiftCoins), ex);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { ex.Message });
            }
            catch (ApplicationException ex)
            {
                await LogError(refLink, request, request.RecipientClientId, nameof(ClaimGiftCoins), ex);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { ex.Message });
            }
            catch (Exception ex)
            {
                await LogError(refLink, request, request.RecipientClientId, nameof(ClaimGiftCoins), ex);
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
                return NotFound("Not a new client.");
            }            

            var refLink = await _referralLinksService.GetReferralLinkById(request.ReferalLinkId);

            var validationError = ValidateClaimGiftCoinsRequest(refLink);
            if (!String.IsNullOrEmpty(validationError))
            {
                return BadRequest(validationError);
            }

            var claims = await _referralLinkClaimsService.GetRefLinkClaims(refLink.Id);

            bool shouldReceiveReward = ShoulReceiveReward(claims, refLink);

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
            }
            else
            {
                await _log.WriteErrorAsync(ControllerContext.GetExecutongControllerAndAction(), nameof(SetRefLinkClaimTransactionId), (new { result, refLinkClaim }).ToJson(), new Exception(result.ToJson()), DateTime.Now);
            }
        }

        private bool ShoulReceiveReward(IEnumerable<IReferralLinkClaim> claims, IReferralLink refLink)
        {
            return claims.Where(c => c.IsNewClient && c.ShouldReceiveReward && c.RecipientClientId != refLink.SenderClientId).Count() > _settings.InvitationLinkSettings.MaxNumOfClientsToReceiveReward;
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

        private async Task<ExchangeOperationResult> TransferRewardCoins(IReferralLink refLink, ClaimReferralLinkRequest request, string recipientClientId)
        {
            try
            {
                var result = await _exchangeOperationsService.TransferAsync(
                         recipientClientId,
                         _settings.LykkeReferralClientId,
                         (double)refLink.Amount,
                         refLink.Asset,
                         TransferType.Common.ToString()
                         );

                return result;
            }
            catch (OffchainException ex)
            {
                await LogError(refLink, request, recipientClientId, nameof(TransferRewardCoins), ex);
                return new ExchangeOperationResult { };
            }
            catch (ApplicationException ex)
            {
                await LogError(refLink, request, recipientClientId, nameof(TransferRewardCoins), ex);
                return new ExchangeOperationResult { };
            }
            catch (Exception ex)
            {
                await LogError(refLink, request, recipientClientId, nameof(TransferRewardCoins), ex);
                return new ExchangeOperationResult { };
            }
        }

        private async Task LogError(IReferralLink refLink, ClaimReferralLinkRequest claimRequest, string recipientClientId, string method, Exception ex)
        {
            await _log.WriteErrorAsync(ControllerContext.GetExecutongControllerAndAction(), method, (new { refLink, claimRequest, recipientClientId }).ToJson(), ex, DateTime.Now);            
        }
    }
}


//try
//{
//    using (TransactionScope scope = new TransactionScope())
//    {
//        var response = await _exchangeOperationsService.TransferAsync(
//           request.RecipientClientId,
//           _settings.LykkeReferralClientId, 
//           (double)refLink.Amount,
//           asset.Id,
//           TransferType.Common.ToString()
//           );

//        if (response.IsOk())
//        {
//            await _referralLinkClaimsService.CreateAsync(new ReferralLinkClaim
//            {
//                IsNewClient = request.IsNewClient,
//                RecipientClientId = request.RecipientClientId,
//                RecipientTransactionId = response.TransactionId,
//                ReferralLinkId = refLink.Id,
//                ShouldReceiveReward = true
//            });

//            refLink.State = ReferralLinkState.Claimed;
//            await _referralLinksService.UpdateAsync(refLink);

//            scope.Complete();

//            return Ok(new TransferFromLykkeWalletResponseModel
//            {
//                TransactionId = response.TransactionId,
//                Message = response.Message
//            });
//        }
//        else
//        {
//            scope.Dispose();
//            return NotFound(new ErrorResponse($"Error transfering money from shared lykke wallet to claiming client: {response.Message}", response.Code?.ToString()));
//        }
//    }                
//}
//catch (OffchainException ex)
//{
//    return NotFound(new ErrorResponse(ex.OffchainExceptionMessage, ex.OffchainExceptionCode));
//}
//catch (TransactionAbortedException ex)
//{
//    return NotFound(new ErrorResponse(ex.Message, ""));
//}
//catch (ApplicationException ex)
//{
//    return NotFound(new ErrorResponse(ex.Message, ""));
//}


//private async Task<ObjectResult> TransferRewardCoinsToRecipient(IReferralLink refLink, string recipientClientId, bool shouldReceiveReward, bool isNewClient)
//{
//    try
//    {
//        //using (TransactionScope scope = new TransactionScope())
//        //{


//            ExchangeOperationResult response = null;
//            if (shouldReceiveReward)
//            {

//            }

//            if ((shouldReceiveReward && response != null && response.IsOk()) || !shouldReceiveReward)
//            {


//                if (refLink.Type == ReferralLinkType.MoneyTransfer)
//                {
//                    refLink.State = ReferralLinkState.Claimed;
//                }

//                await _referralLinksService.UpdateAsync(refLink);

//                scope.Complete();

//                return Ok(new TransferFromLykkeWalletResponseModel
//                {
//                    TransactionId = response?.TransactionId,
//                    Message = response?.Message
//                });

//            }
//            else
//            {
//                scope.Dispose();
//                return NotFound(new ErrorResponse($"Error transfering money from shared lykke wallet to claiming client: {response.Message}", response.Code?.ToString()));
//            }
//        //}
//    }
//    catch (OffchainException ex)
//    {
//        return NotFound(new ErrorResponse(ex.OffchainExceptionMessage, ex.OffchainExceptionCode));
//    }
//    catch (TransactionAbortedException ex)
//    {
//        return NotFound(new ErrorResponse(ex.Message, ""));
//    }
//    catch (ApplicationException ex)
//    {
//        return NotFound(new ErrorResponse(ex.Message, ""));
//    }
//}


//private async Task<ObjectResult> TransferRewardCoinsToRecipient(IReferralLink refLink, string recipientClientId, bool shouldReceiveReward, bool isNewClient)
//{
//    try
//    {
//        using (TransactionScope scope = new TransactionScope())
//        {
//            ExchangeOperationResult response = null;

//            if (shouldReceiveReward)
//            {
//                response = await _exchangeOperationsService.TransferAsync(
//                   recipientClientId, //request.RecipientClientId,
//                   _settings.LykkeReferralClientId,
//                   (double)refLink.Amount,
//                   refLink.Asset,
//                   TransferType.Common.ToString()
//                   );
//            }

//            if ((shouldReceiveReward && response != null && response.IsOk()) || !shouldReceiveReward)
//            {
//                await _referralLinkClaimsService.CreateAsync(new ReferralLinkClaim
//                {
//                    IsNewClient = isNewClient, //request.IsNewClient,
//                    RecipientClientId = recipientClientId, //request.RecipientClientId,
//                    RecipientTransactionId = response?.TransactionId,
//                    ReferralLinkId = refLink.Id,
//                    ShouldReceiveReward = shouldReceiveReward
//                });

//                if (refLink.Type == ReferralLinkType.MoneyTransfer)
//                {
//                    refLink.State = ReferralLinkState.Claimed;
//                }

//                await _referralLinksService.UpdateAsync(refLink);

//                scope.Complete();

//                return Ok(new TransferFromLykkeWalletResponseModel
//                {
//                    TransactionId = response?.TransactionId,
//                    Message = response?.Message
//                });

//            }
//            else
//            {
//                scope.Dispose();
//                return NotFound(new ErrorResponse($"Error transfering money from shared lykke wallet to claiming client: {response.Message}", response.Code?.ToString()));
//            }
//        }
//    }
//    catch (OffchainException ex)
//    {
//        return NotFound(new ErrorResponse(ex.OffchainExceptionMessage, ex.OffchainExceptionCode));
//    }
//    catch (TransactionAbortedException ex)
//    {
//        return NotFound(new ErrorResponse(ex.Message, ""));
//    }
//    catch (ApplicationException ex)
//    {
//        return NotFound(new ErrorResponse(ex.Message, ""));
//    }
//}
