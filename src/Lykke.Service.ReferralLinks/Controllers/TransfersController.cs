using Common;
using Common.Log;
using Core.BitCoin.BitcoinApi.Models;
using Lykke.Service.ExchangeOperations.Client;
using Lykke.Service.Kyc.Client;
using Lykke.Service.ReferralLinks.Core.BitCoinApi.Models;
using Lykke.Service.ReferralLinks.Core.Domain.Client;
using Lykke.Service.ReferralLinks.Core.Domain.Exceptions;
using Lykke.Service.ReferralLinks.Core.Domain.Offchain;
using Lykke.Service.ReferralLinks.Core.Domain.ReferralLink;
using Lykke.Service.ReferralLinks.Core.Kyc;
using Lykke.Service.ReferralLinks.Core.Services;
using Lykke.Service.ReferralLinks.Core.Settings.ServiceSettings;
using Lykke.Service.ReferralLinks.Models;
using Lykke.Service.ReferralLinks.Models.Offchain;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.SwaggerGen.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.ReferralLinks.Controllers
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

        [HttpGet("channelKey")]
        public async Task<IActionResult> GetEncryptedKey([FromQuery] string asset, [FromQuery] string clientId)
        {
            var data = await _offchainEncryptedKeysRepository.GetKey(clientId, asset);

            await LogInfo(new { asset, clientId }, ControllerContext, $"Channel key: {data?.Key}");

            return Ok(new OffchainEncryptedKeyRespModel
            {
                Key = data?.Key
            });
        }

        [HttpPost("transferToLykkeWallet")]        
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
                return await LogAndReturnInternalServerError(model, ControllerContext, ex);
            }
        }

        
        [HttpPost("processChannel")]
        public async Task<IActionResult> ProcessChannel([FromBody] OffchainChannelProcessModel model)
        {
            var clientId = model.ClientId;

            if (string.IsNullOrEmpty(model.SignedChannelTransaction))
                return BadRequest(new ErrorResponse("SignedChannelTransaction must not be empty", ""));

            if (string.IsNullOrEmpty(model.TransferId))
                return BadRequest(new ErrorResponse("TransferId must not be empty", ""));

            try
            {
                var response = await _offchainService.CreateHubCommitment(clientId, model.TransferId, model.SignedChannelTransaction);

                return Ok(new OffchainTradeRespModel
                {
                    TransferId = response.TransferId,
                    TransactionHex = response.TransactionHex,
                    OperationResult = response.OperationResult
                });
            }
            //TODO
            catch (OffchainException ex)
            {
                return NotFound();
                //return NotFound(ProcessError(ex));
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
        }

        [HttpPost("finalizeRefLinkTransfer")]
        public async Task<IActionResult> Finalize([FromBody] OffchainFinalizeModel model)
        {
            var clientId = model.ClientId;

            await CheckOffchain(clientId);            

            if (string.IsNullOrEmpty(model.ClientRevokePubKey))
                return BadRequest(new ErrorResponse("ClientRevokePubKey must not be empty", ""));

            if (string.IsNullOrEmpty(model.SignedTransferTransaction))
                return BadRequest(new ErrorResponse("SignedTransferTransaction must not be empty", ""));

            if (string.IsNullOrEmpty(model.TransferId))
                return BadRequest(new ErrorResponse("TransferId must not be empty", ""));

            var refLinkEntity = await _referralLinksService.GetReferralLinkById(model.RefLinkId);
            if (refLinkEntity == null)
            {
                return BadRequest(new ErrorResponse("RefLinkId not found", ""));
            }

            try
            {
                var response = await _offchainService.Finalize(clientId, model.TransferId, model.ClientRevokePubKey,
                    model.ClientRevokeEncryptedPrivateKey, model.SignedTransferTransaction);
                
                if(response!= null && response.OperationResult == OffchainOperationResult.ClientCommitment)
                {
                    AttachSenderTransferToRefLink(refLinkEntity, response.TransferId);
                }                

                var request =
                    (await _offchainRequestRepository.GetRequestsForClient(clientId)).FirstOrDefault(
                        x => x.TransferId == model.TransferId);                

                if (request != null)
                    await _offchainRequestRepository.Complete(request.RequestId);

                var offchainOrder = await _offchainService.GetResultOrderFromTransfer(model.TransferId);

                return Ok(new OffchainSuccessTradeRespModel
                {
                    TransferId = response.TransferId,
                    TransactionHex = response.TransactionHex,
                    OperationResult = response.OperationResult,
                    Order = offchainOrder != null ? ConvertToApi(offchainOrder) : null
                });
            }
            //TODO
            catch (OffchainException ex)
            {
                return NotFound();
                //return NotFound(ProcessError(ex));
            }
            catch (TradeException ex)
            {
                var msg = "";
                switch (ex.Type)
                {
                    case TradeExceptionType.LeadToNegativeSpread:
                        msg = "LimitOrderLeadToNegativeSpread";
                        break;
                    default:
                        msg = "TechnicalProblems";
                        break;
                }
                return NotFound(new ErrorResponse(msg, ""));
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


        //private ErrorResponse ProcessError(OffchainException ex)
        //{
        //    return new ErrorResponse(ex.OffchainExceptionMessage, ex.OffchainExceptionCode);
        //}
    }
}
