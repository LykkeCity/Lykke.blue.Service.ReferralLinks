using Common;
using Common.Log;
using Core.BitCoin.BitcoinApi.Models;
using Lykke.Service.ExchangeOperations.Client;
using Lykke.Service.Kyc.Client;
using Lykke.blue.Service.ReferralLinks.Core.BitCoinApi.Models;
using Lykke.blue.Service.ReferralLinks.Core.Domain.Client;
using Lykke.blue.Service.ReferralLinks.Core.Domain.Exceptions;
using Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain;
using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using Lykke.blue.Service.ReferralLinks.Core.Kyc;
using Lykke.blue.Service.ReferralLinks.Core.Services;
using Lykke.blue.Service.ReferralLinks.Core.Settings.ServiceSettings;
using Lykke.blue.Service.ReferralLinks.Models;
using Lykke.blue.Service.ReferralLinks.Models.Offchain;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.SwaggerGen.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.Controllers
{
    [Route("api/transfers")]
    public class TransfersController : RefLinksBaseController
    {
        private readonly ISrvKycForAsset _srvKycForAsset;
        private readonly IClientSettingsRepository _clientSettingsRepository;
        private readonly IOffchainService _offchainService;
        private readonly IExchangeOperationsServiceClient _exchangeOperationsService;
        private readonly ReferralLinksSettings _settings;
        private readonly IOffchainRequestRepository _offchainRequestRepository;
        private readonly IOffchainTransferRepository _offchainTransferRepository;
        private readonly ILog _log;
        private readonly IOffchainEncryptedKeysRepository _offchainEncryptedKeysRepository;        
        private readonly IReferralLinksService _referralLinksService;
        private readonly CachedDataDictionary<string, Lykke.Service.Assets.Client.Models.Asset> _assets;

        public TransfersController(ISrvKycForAsset srvKycForAsset, 
            IClientSettingsRepository clientSettingsRepository, 
            IOffchainService offchainService, 
            ReferralLinksSettings settings, 
            ILog log, 
            IExchangeOperationsServiceClient exchangeOperationsService, 
            IOffchainEncryptedKeysRepository offchainEncryptedKeysRepository, 
            IOffchainRequestRepository offchainRequestRepository, 
            IReferralLinksService referralLinksService,
            IOffchainTransferRepository offchainTransferRepository,
            CachedDataDictionary<string, Lykke.Service.Assets.Client.Models.Asset> assets) : base (log)
        {
            _srvKycForAsset = srvKycForAsset;
            _clientSettingsRepository = clientSettingsRepository;
            _offchainService = offchainService;
            _settings = settings;
            _log = log;
            _exchangeOperationsService = exchangeOperationsService;
            _offchainEncryptedKeysRepository = offchainEncryptedKeysRepository;
            _offchainRequestRepository = offchainRequestRepository;
            _referralLinksService = referralLinksService;
            _offchainTransferRepository = offchainTransferRepository;
            _assets = assets;
        }

        protected async Task CheckOffchain(string clientId)
        {
            if (!await _clientSettingsRepository.IsOffchainClient(clientId))
                throw new Exception("Offchain is not supported");
        }



        /// <summary>
        /// Get offchain ChannelKey for transfer.  
        /// </summary>
        /// <returns></returns>
        [HttpPost("channelKey")]
        [SwaggerOperation("GetChannelKey")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetChannelKey([FromBody] OffchainGetChannelKeyRequest request)
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
        [HttpPost("transferToLykkeWallet")]
        [SwaggerOperation("TransferToLykkeWallet")]
        [ProducesResponseType(typeof(OffchainTradeRespModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> TransferToLykkeWallet([FromBody] TransferToLykkeWallet model)
        {
            var clientId = model.ClientId;
            var refLink = await _referralLinksService.GetReferralLinkById(model.ReferralLinkId);

            if(refLink == null)
            {
                await LogAndReturnBadRequest(model, ControllerContext, "Ref link Id not found ot missing");
            }

            var asset = (await _assets.GetDictionaryAsync()).Values.Where(v => v.Symbol == refLink.Asset).FirstOrDefault();

            if(asset == null)
            {
                await LogAndReturnBadRequest(model, ControllerContext, $"Specified asset {refLink.Asset} in reflink id {refLink.Id} not found ");
            }

            await CheckOffchain(clientId);

            if (await _srvKycForAsset.IsKycNeeded(clientId, asset.Id))
            {
                return await LogAndReturnBadRequest(model, ControllerContext, $"KYC needed for sender client id {model.ClientId} before claiming asset {refLink.Asset}");
            }               

            try
            {
                var response = await _offchainService.CreateDirectTransfer(clientId, asset.Id, (decimal) refLink.Amount, model.PrevTempPrivateKey);

                var exchangeOpResult = await _exchangeOperationsService.StartTransferAsync(
                    response.TransferId,
                    _settings.LykkeReferralClientId, //send to shared lykke wallet where coins will be temporary stored until claimed by the recipient
                    clientId,
                    TransferType.Common.ToString()
                    );

                await LogInfo(new { Method = "StartTransferAsync", TransferId = response.TransferId, SourceClientId = clientId }, ControllerContext, exchangeOpResult.ToJson());

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
        [HttpPost("processChannel")]
        [SwaggerOperation("ProcessChannel")]
        [ProducesResponseType(typeof(OffchainTradeRespModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.NotFound)]
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
                return await LogAndReturnNotFound(request, ControllerContext, ex.Message);
            }
        }

        private async void AttachSenderTransferToRefLink(IReferralLink refLink, string transferId)
        {
            var transfer = await _offchainTransferRepository.GetTransfer(transferId);

            refLink.Amount = (double)transfer.Amount;
            refLink.Asset = (await _assets.GetItemAsync(transfer.AssetId)).Symbol;
            refLink.SenderOffchainTransferId = transferId;
            refLink.State = ReferralLinkState.SentToLykkeSharedWallet.ToString();

            await _referralLinksService.UpdateAsync(refLink);

            await LogInfo(new { RefLink = refLink, TransferId = transferId }, ControllerContext, $"Transfer complete for ref link id {refLink.Id} with amount {transfer.Amount} and asset {refLink.Asset}. Offchain transfer Id {transferId} attached with ref link. ");
        }

        /// <summary>
        /// Process offchain channel
        /// </summary>
        /// <returns></returns>
        [HttpPost("finalizeRefLinkTransfer")]
        [SwaggerOperation("FinalizeRefLinkTransfer")]
        [ProducesResponseType(typeof(OffchainSuccessTradeRespModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Finalize([FromBody] OffchainFinalizeModel request)
        {
            var clientId = request.ClientId;

            await CheckOffchain(clientId);            

            if (string.IsNullOrEmpty(request.ClientRevokePubKey))
            {
                await LogAndReturnBadRequest(request, ControllerContext, "ClientRevokePubKey must not be empty");
            }

            if (string.IsNullOrEmpty(request.SignedTransferTransaction))
            {
                await LogAndReturnBadRequest(request, ControllerContext, "SignedTransferTransaction must not be empty");
            }

            if (string.IsNullOrEmpty(request.TransferId))
            {
                await LogAndReturnBadRequest(request, ControllerContext, "TransferId must not be empty");
            }

            var refLinkEntity = await _referralLinksService.GetReferralLinkById(request.RefLinkId);
            if (refLinkEntity == null)
            {
                await LogAndReturnBadRequest(request, ControllerContext, "RefLinkId not found");
            }

            try
            {
                var response = await _offchainService.Finalize(clientId, request.TransferId, request.ClientRevokePubKey, request.ClientRevokeEncryptedPrivateKey, request.SignedTransferTransaction);
                
                if(response!= null && response.OperationResult == OffchainOperationResult.ClientCommitment)
                {
                    AttachSenderTransferToRefLink(refLinkEntity, response.TransferId);
                }
                else
                {
                    await LogWarn(request, ControllerContext, $"_offchainService.Finalize returned unexpected result :  {response?.ToJson()}");                    
                }

                var offchainRequest =
                    (await _offchainRequestRepository.GetRequestsForClient(clientId)).FirstOrDefault(
                        x => x.TransferId == request.TransferId);                

                if (offchainRequest != null)
                {
                    await _offchainRequestRepository.Complete(offchainRequest.RequestId);
                    await LogInfo(request, ControllerContext, $"Offchain request set to complete: {offchainRequest.ToJson()}");
                }                    

                var offchainOrder = await _offchainService.GetResultOrderFromTransfer(request.TransferId);

                return Ok(new OffchainSuccessTradeRespModel
                {
                    TransferId = response.TransferId,
                    TransactionHex = response.TransactionHex,
                    OperationResult = response.OperationResult,
                    Order = offchainOrder != null ? ConvertToApi(offchainOrder) : null
                });
            }
            catch (OffchainException ex)
            {
                return await LogOffchainExceptionAndReturn(request, ControllerContext, ex);
            }
            catch (TradeException ex)
            {
               return await LogTraderExceptionAndReturn(request, ControllerContext, ex);
            }
            catch (Exception ex)
            {
                return await LogAndReturnNotFound(request, ControllerContext, ex.Message);
            }
        }

        private ApiOffchainOrder ConvertToApi(OffchainResultOrder order)
        {
            return new ApiOffchainOrder
            {
                AssetPair = order.AssetPair,
                Asset = order.Asset,
                Id = order.Id,
                DateTime = order.DateTime.ToIsoDateTime(),
                OrderType = order.OrderType.ToString(),
                Price = order.Price,
                Volume = Math.Abs(order.Volume),
                TotalCost = Math.Abs(order.TotalCost)
            };
        }
    }
}
