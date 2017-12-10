using Common;
using Common.Log;
using Lykke.blue.Service.ReferralLinks.Core.Domain.Client;
using Lykke.blue.Service.ReferralLinks.Core.Domain.ExchangeOperations;
using Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain;
using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using Lykke.blue.Service.ReferralLinks.Core.Kyc;
using Lykke.blue.Service.ReferralLinks.Core.Services;
using Lykke.blue.Service.ReferralLinks.Core.Settings;
using Lykke.blue.Service.ReferralLinks.Models.Offchain;
using Lykke.Service.ExchangeOperations.Client;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.Controllers
{
    //[Route("api/transfers")] //--reserved for version 2
    public class TransfersController : RefLinksBaseController
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

        public TransfersController(ISrvKycForAsset srvKycForAsset,
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

        private async Task CheckOffchain(string clientId)
        {
            if (!await _clientSettingsRepository.IsOffchainClient(clientId))
                throw new Exception("Offchain is not supported");
        }



        /// <summary>
        /// Get offchain ChannelKey for transfer.  
        /// </summary>
        /// <returns></returns>
        //[HttpPost("channelKey")] --reserved for version 2
        //[SwaggerOperation("GetChannelKey")]
        //[ProducesResponseType(typeof(OffchainEncryptedKeyRespModel), (int)HttpStatusCode.OK)]
        private async Task<IActionResult> GetChannelKey([FromBody] OffchainGetChannelKeyRequest request)
        {
            var data = await _offchainEncryptedKeysRepository.GetKey(request.ClientId, request.Asset);

            await LogInfo(new { request.Asset, request.ClientId }, ControllerContext, $"Channel key: {data?.Key}");

            return Ok(new OffchainEncryptedKeyRespModel
            {
                Key = data?.Key
            });
        }


        /// <summary>
        /// Create offchain transfer to Lykke wallet
        /// </summary>
        /// <returns></returns>
        //[HttpPost("transferToLykkeWallet")] --reserved for version 2
        //[SwaggerOperation("TransferToLykkeWallet")]
        //[ProducesResponseType(typeof(OffchainTradeRespModel), (int)HttpStatusCode.OK)]
        //[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
        //[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.NotFound)]
        private async Task<IActionResult> TransferToLykkeWallet([FromBody] TransferToLykkeWallet model)
        {
            var clientId = model.ClientId;
            var refLink = await _referralLinksService.GetReferralLinkById(model.ReferralLinkId);

            if (refLink == null)
            {
                return await LogAndReturnBadRequest(model, ControllerContext, "Ref link Id not found ot missing");
            }

            var asset = (await _assets.GetDictionaryAsync()).Values.FirstOrDefault(v => v.Id == refLink.Asset);

            if (asset == null)
            {
                return await LogAndReturnBadRequest(model, ControllerContext, $"Specified asset id {refLink.Asset} in reflink id {refLink.Id} not found ");
            }

            await CheckOffchain(clientId);

            if (await _srvKycForAsset.IsKycNeeded(clientId, asset.Id))
            {
                return await LogAndReturnBadRequest(model, ControllerContext, $"KYC needed for sender client id {model.ClientId} before sending asset {refLink.Asset}");
            }

            try
            {
                var response = await _offchainService.CreateDirectTransfer(clientId, asset.Id, (decimal)refLink.Amount, model.PrevTempPrivateKey);

                var exchangeOpResult = await _exchangeOperationsService.StartTransferAsync(
                    response.TransferId,
                    _settings.ReferralLinksService.LykkeReferralClientId, //send to shared lykke wallet where coins will be temporary stored until claimed by the recipient
                    clientId,
                    TransferType.Common.ToString()
                    );

                await LogInfo(new
                {
                    Method = "StartTransferAsync",
                    response.TransferId,
                    SourceClientId = clientId
                }, ControllerContext, exchangeOpResult.ToJson());

                return Ok(new OffchainTradeRespModel
                {
                    TransferId = response.TransferId,
                    TransactionHex = response.TransactionHex,
                    OperationResult = response.OperationResult
                });
            }
            catch (OffchainException ex)
            {
                return await LogOffchainExceptionAndReturn(model, ControllerContext, ex);
            }
            catch (Exception ex)
            {
                return await LogAndReturnNotFound(model, ControllerContext, ex.Message);
            }
        }




        /// <summary>
        /// Process offchain channel
        /// </summary>
        /// <returns></returns>
        //[HttpPost("processChannel")] --reserved for version 2
        //[SwaggerOperation("ProcessChannel")]
        //[ProducesResponseType(typeof(OffchainTradeRespModel), (int)HttpStatusCode.OK)]
        //[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
        //[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.NotFound)]
        private async Task<IActionResult> ProcessChannel([FromBody] OffchainChannelProcessModel request)
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
                return await LogAndReturnNotFound(request, ControllerContext, ex.Message);
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

            await LogInfo(new { RefLink = refLink, TransferId = transferId }, ControllerContext, $"Transfer complete for ref link id {refLink.Id} with amount {transfer.Amount} and asset Id {refLink.Asset}. Offchain transfer Id {transferId} attached with ref link. ");
        }

        /// <summary>
        /// Process offchain channel
        /// </summary>
        /// <returns></returns>
        //[HttpPost("finalizeRefLinkTransfer")] --reserved for version 2
        //[SwaggerOperation("FinalizeRefLinkTransfer")]
        //[ProducesResponseType(typeof(OffchainSuccessTradeRespModel), (int)HttpStatusCode.OK)]
        //[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
        //[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.NotFound)]
        private async Task<IActionResult> Finalize([FromBody] OffchainFinalizeModel request)
        {
            var clientId = request.ClientId;

            await CheckOffchain(clientId);

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

            var refLinkEntity = await _referralLinksService.GetReferralLinkById(request.RefLinkId);
            if (refLinkEntity == null)
            {
                return await LogAndReturnBadRequest(request, ControllerContext, "RefLinkId not found");
            }

            try
            {
                var response = await _offchainService.Finalize(clientId, request.TransferId, request.ClientRevokePubKey, request.ClientRevokeEncryptedPrivateKey, request.SignedTransferTransaction);

                if (response == null)
                {
                    return await LogAndReturnNotFound(request, ControllerContext, "OffchainService Finalize returned NULL. Can not finalize transfer.");
                }

                if (response.OperationResult == OffchainOperationResult.ClientCommitment)
                {
                    AttachSenderTransferToRefLink(refLinkEntity, response.TransferId);
                }
                else
                {
                    await LogWarn(request, ControllerContext, $"_offchainService.Finalize returned unexpected result :  {response.ToJson()}");
                }

                var offchainRequest =
                    (await _offchainRequestRepository.GetRequestsForClient(clientId)).FirstOrDefault(
                        x => x.TransferId == request.TransferId);

                if (offchainRequest != null)
                {
                    await _offchainRequestRepository.Complete(offchainRequest.RequestId);
                    await LogInfo(request, ControllerContext, $"Offchain request set to complete: {offchainRequest.ToJson()}");
                }

                return Ok(new OffchainSuccessTradeRespModel
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
                return await LogAndReturnNotFound(request, ControllerContext, ex.Message);
            }
        }
    }
}
