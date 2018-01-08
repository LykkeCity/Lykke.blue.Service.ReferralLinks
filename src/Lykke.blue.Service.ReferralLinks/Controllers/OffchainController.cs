using Common;
using Common.Log;
using Lykke.blue.Service.ReferralLinks.Core.Domain.Client;
using Lykke.blue.Service.ReferralLinks.Core.Domain.ExchangeOperations;
using Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain;
using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using Lykke.blue.Service.ReferralLinks.Core.Kyc;
using Lykke.blue.Service.ReferralLinks.Core.Services;
using Lykke.blue.Service.ReferralLinks.Core.Settings;
using Lykke.blue.Service.ReferralLinks.Models;
using Lykke.blue.Service.ReferralLinks.Models.Offchain;
using Lykke.Service.ExchangeOperations.Client;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

//OffchainService and all offchain functionality will be removed in next PR. No need to review.

namespace Lykke.blue.Service.ReferralLinks.Controllers
{
    [Route("api/offchain")] 
    public class OffchainController : RefLinksBaseController
    {
        private readonly ISrvKycForAsset _srvKycForAsset;
        private readonly IClientSettingsRepository _clientSettingsRepository;
        private readonly IOffchainService _offchainService;
        private readonly IExchangeOperationsServiceClient _exchangeOperationsService;
        private readonly AppSettings _settings;
        private readonly IOffchainRequestRepository _offchainRequestRepository;
        private readonly IOffchainTransferRepository _offchainTransferRepository;
        private readonly IOffchainEncryptedKeysRepository _offchainEncryptedKeysRepository;
        private readonly IReferralLinksService _referralLinksService;
        private readonly CachedDataDictionary<string, Lykke.Service.Assets.Client.Models.Asset> _assets;

        public OffchainController(ISrvKycForAsset srvKycForAsset,
            IClientSettingsRepository clientSettingsRepository,
            IOffchainService offchainService,
            AppSettings settings,
            ILog log,
            IExchangeOperationsServiceClient exchangeOperationsService,
            IOffchainEncryptedKeysRepository offchainEncryptedKeysRepository,
            IOffchainRequestRepository offchainRequestRepository,
            IReferralLinksService referralLinksService,
            IOffchainTransferRepository offchainTransferRepository,
            CachedDataDictionary<string, Lykke.Service.Assets.Client.Models.Asset> assets) : base(log)
        {
            _srvKycForAsset = srvKycForAsset;
            _clientSettingsRepository = clientSettingsRepository;
            _offchainService = offchainService;
            _settings = settings;
            _exchangeOperationsService = exchangeOperationsService;
            _offchainEncryptedKeysRepository = offchainEncryptedKeysRepository;
            _offchainRequestRepository = offchainRequestRepository;
            _referralLinksService = referralLinksService;
            _offchainTransferRepository = offchainTransferRepository;
            _assets = assets;
        }

        private async Task<bool> CheckOffchain(string clientId)
        {
            return await _clientSettingsRepository.IsOffchainClient(clientId);
        }

        /// <summary>
        /// Get offchain ChannelKey for transfer.  
        /// </summary>
        /// <returns></returns>
        [HttpGet("channel/key/{asset}/{clientId}")]
        [SwaggerOperation("GetChannelKey")]
        [ProducesResponseType(typeof(OffchainEncryptedKeyRespModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetChannelKey(string asset, string clientId)
        {
            var data = await _offchainEncryptedKeysRepository.GetKey(clientId, asset);

            await LogInfo(new { asset, clientId }, ControllerContext, $"Channel key: {data?.Key}");

            return Ok(new OffchainEncryptedKeyRespModel
            {
                Key = data?.Key
            });
        }

        private async Task<IActionResult> CreateOffChainTransfer(string clientId, string assetId, decimal amount, string prevTempPrivateKey, string refLinkGroupId)
        {
            var request = new { clientId, assetId, amount, prevTempPrivateKey, refLinkGroupId };

            try
            {
                if (!await CheckOffchain(clientId))
                {
                    return await LogAndReturnBadRequest(request, ControllerContext, $"ClientId {clientId} is not an offchain client");
                }

                var response = await _offchainService.CreateDirectTransfer(clientId, assetId, amount, prevTempPrivateKey);

                var exchangeOpResult = await _exchangeOperationsService.StartTransferAsync(
                    response.TransferId,
                    _settings.ReferralLinksService.LykkeReferralClientId, //send to shared lykke wallet where coins will be temporary stored until claimed by the recipient
                    clientId,
                    TransferType.Common.ToString()
                );

                await LogInfo(new { RefLinkGroupId = refLinkGroupId, Method = nameof(CreateOffChainTransfer), OffChainTransferId = response.TransferId, SourceClientId = clientId }, ControllerContext, exchangeOpResult.ToJson());

                return Ok(new OffchainTradeRespModel
                {
                    TransferId = response.TransferId,
                    TransactionHex = response.TransactionHex,
                    OperationResult = response.OperationResult
                });
            }
            catch (OffchainException ex)
            {
                return await LogOffchainExceptionAndReturn(request, ControllerContext, ex);
            }
            catch (Exception ex)
            {
                return await LogAndReturnInternalServerError(request, ControllerContext, ex);
            }
        }
        

        /// <summary>
        /// Create offchain transfer to Lykke wallet
        /// </summary>
        /// <returns></returns>
        [HttpPost("transfer")]
        [SwaggerOperation("TransferToLykkeHotWallet")]
        [ProducesResponseType(typeof(OffchainTradeRespModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> TransferToLykkeWallet([FromBody] OffchainTransferToLykkeModel model)
        {
            var refLink = await _referralLinksService.Get(model.ReferralLinkId);

            if (refLink == null)
            {
                return await LogAndReturnBadRequest(model, ControllerContext, "Ref link Id not found or missing");
            }

            var clientId = refLink.SenderClientId;
            var refLinkAsset = refLink.Asset;

            if (refLink.Amount <= 0 || !await _referralLinksService.HasEnoughBalance(clientId, refLinkAsset, refLink.Amount))
            {
                return await LogAndReturnBadRequest(model, ControllerContext, "Not enough balance.");
            }

            return await CreateOffChainTransfer(clientId, refLinkAsset, (decimal) refLink.Amount, model.PrevTempPrivateKey, model.ReferralLinkId);
        }

        /// <summary>
        /// Process offchain channel
        /// </summary>
        /// <returns></returns>
        [HttpPost("channel/process")]
        [SwaggerOperation("ProcessChannel")]
        [ProducesResponseType(typeof(OffchainTradeRespModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ProcessChannel([FromBody] OffchainChannelProcessModel request)
        {
            var clientId = request.ClientId;

            if (string.IsNullOrEmpty(request.SignedChannelTransaction))
            {
                await LogAndReturnBadRequest(request, ControllerContext, "SignedChannelTransaction must not be empty");
            }

            if (string.IsNullOrEmpty(request.TransferId))
            {
                await LogAndReturnBadRequest(request, ControllerContext, "TransferId must not be empty");
            }

            try
            {
                var response = await _offchainService.CreateHubCommitment(clientId, request.TransferId, request.SignedChannelTransaction);

                return Ok(new OffchainTradeRespModel
                {
                    TransferId = response.TransferId,
                    TransactionHex = response.TransactionHex,
                    OperationResult = response.OperationResult
                });
            }
            catch (OffchainException ex)
            {
                return await LogOffchainExceptionAndReturn(request, ControllerContext, ex);
            }
            catch (Exception ex)
            {
                return await LogAndReturnInternalServerError(request, ControllerContext, ex);
            }
        }

        /// <summary>
        /// Process offchain channel
        /// </summary>
        /// <returns></returns>
        [HttpPost("transfer/finalize")]
        [SwaggerOperation("FinalizeRefLinkTransfer")]
        [ProducesResponseType(typeof(OffchainTradeRespModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Finalize([FromBody] OffchainFinalizeModel request)
        {
            var clientId = request.ClientId;

            if (string.IsNullOrEmpty(request.ClientRevokePubKey))
            {
                return await LogAndReturnBadRequest(request, ControllerContext, "ClientRevokePubKey must not be empty");
            }

            if (string.IsNullOrEmpty(request.SignedTransferTransaction))
            {
                return await LogAndReturnBadRequest(request, ControllerContext, "SignedTransferTransaction must not be empty");
            }

            if (string.IsNullOrEmpty(request.TransferId))
            {
                return await LogAndReturnBadRequest(request, ControllerContext, "TransferId must not be empty");
            }

            var refLinkEntity = await _referralLinksService.Get(request.RefLinkId);
            if (refLinkEntity == null)
            {
                return await LogAndReturnBadRequest(request, ControllerContext, "RefLinkId not found");
            }

            try
            {
                if (!await CheckOffchain(clientId))
                {
                    return await LogAndReturnBadRequest(request, ControllerContext, $"ClientId {clientId} is not an offchain client");
                }

                var response = await _offchainService.Finalize(clientId, request.TransferId, request.ClientRevokePubKey, request.ClientRevokeEncryptedPrivateKey, request.SignedTransferTransaction);

                if (response == null)
                {
                    return await LogAndReturnInternalServerError(request, ControllerContext, "OffchainService Finalize returned NULL. Can not finalize transfer.");
                }

                await LogInfo(request, ControllerContext, $"OffChain finalize result: {response.ToJson()}");

                if (response.OperationResult == OffchainOperationResult.ClientCommitment)
                {
                    AttachSenderTransferToRefLink(refLinkEntity, response.TransferId);
                }
                else
                {

                    return await LogAndReturnInternalServerError(request, ControllerContext, $"OffChain finalize returned unexpected result :  {response.ToJson()}");
                }

                var offchainRequest = (await _offchainRequestRepository.GetRequestsForClient(clientId)).FirstOrDefault(x => x.TransferId == request.TransferId);

                if (offchainRequest != null)
                {
                    await _offchainRequestRepository.Complete(offchainRequest.RequestId);
                    await LogInfo(request, ControllerContext, $"Offchain request set to complete: {offchainRequest.ToJson()}");
                }

                return Ok(new OffchainTradeRespModel
                {
                    TransferId = response.TransferId,
                    TransactionHex = response.TransactionHex,
                    OperationResult = response.OperationResult,
                });
            }
            catch (OffchainException ex)
            {
                return await LogOffchainExceptionAndReturn(request, ControllerContext, ex);
            }
            catch (Exception ex)
            {
                return await LogAndReturnInternalServerError(request, ControllerContext, ex);
            }
        }

        private async void AttachSenderTransferToRefLink(IReferralLink refLink, string transferId)
        {
            var transfer = await _offchainTransferRepository.GetTransfer(transferId);

            refLink.Amount = (double)transfer.Amount;
            refLink.Asset = (await _assets.GetItemAsync(transfer.AssetId)).Id;
            refLink.SenderOffchainTransferId = transferId;
            refLink.State = ReferralLinkState.SentToLykkeSharedWallet.ToString();

            await _referralLinksService.UpdateAsync(refLink);

            await LogInfo(new { RefLink = refLink, TransferId = transferId }, ControllerContext, $"GiftCoin transfer to Hot Wallet completed for ref link id {refLink.Id} with amount {transfer.Amount} and asset Id {refLink.Asset}. Offchain transfer Id {transferId} attached to ref link. ");
        }
    }
}
