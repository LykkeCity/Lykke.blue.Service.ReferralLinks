using Common;
using Common.Log;
using Core.BitCoin.BitcoinApi.Models;
using Lykke.Service.ExchangeOperations.Client;
using Lykke.Service.Kyc.Client;
using Lykke.Service.ReferralLinks.Core.BitCoinApi.Models;
using Lykke.Service.ReferralLinks.Core.Domain.Client;
using Lykke.Service.ReferralLinks.Core.Domain.Exceptions;
using Lykke.Service.ReferralLinks.Core.Domain.Offchain;
using Lykke.Service.ReferralLinks.Core.Kyc;
using Lykke.Service.ReferralLinks.Core.Services;
using Lykke.Service.ReferralLinks.Core.Settings.ServiceSettings;
using Lykke.Service.ReferralLinks.Models;
using Lykke.Service.ReferralLinks.Models.Offchain;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.ReferralLinks.Controllers
{
    [Route("api/transfers")]
    public class TransfersController : Controller
    {
        private readonly ISrvKycForAsset _srvKycForAsset;
        private readonly IClientSettingsRepository _clientSettingsRepository;
        private readonly IOffchainService _offchainService;
        private readonly IExchangeOperationsServiceClient _exchangeOperationsService;
        private readonly ReferralLinksSettings _settings;
        private readonly IOffchainRequestRepository _offchainRequestRepository;
        private readonly ILog _log;
        private readonly IOffchainEncryptedKeysRepository _offchainEncryptedKeysRepository;

        public TransfersController(ISrvKycForAsset srvKycForAsset, IClientSettingsRepository clientSettingsRepository, IOffchainService offchainService, ReferralLinksSettings settings, ILog log, IExchangeOperationsServiceClient exchangeOperationsService, IOffchainEncryptedKeysRepository offchainEncryptedKeysRepository, IOffchainRequestRepository offchainRequestRepository)
        {
            _srvKycForAsset = srvKycForAsset;
            _clientSettingsRepository = clientSettingsRepository;
            _offchainService = offchainService;
            _settings = settings;
            _log = log;
            _exchangeOperationsService = exchangeOperationsService;
            _offchainEncryptedKeysRepository = offchainEncryptedKeysRepository;
            _offchainRequestRepository = offchainRequestRepository;
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

            return Ok(new OffchainEncryptedKeyRespModel
            {
                Key = data?.Key
            });
        }


        [HttpPost("transferToLykkeWallet")]
        public async Task<IActionResult> TransferToLykkeWallet([FromBody] TransferToLykkeWallet model)
        {
            var clientId = model.ClientId;

            await CheckOffchain(clientId);

            if (await _srvKycForAsset.IsKycNeeded(clientId, model.Asset))
                return BadRequest(new ErrorResponse("KycNeeded", "")); //ResponseModel<OffchainTradeRespModel>.CreateFail(ResponseModel.ErrorCodeType.InconsistentData, Phrases.KycNeeded);

            try
            {
                var response = await _offchainService.CreateDirectTransfer(clientId, model.Asset, model.Amount, model.PrevTempPrivateKey);

                await _exchangeOperationsService.StartTransferAsync(
                    response.TransferId,
                    _settings.LykkeReferralClientId, //send to shared lykke wallet where coins will be temporary stored until claimed by the recipient
                    clientId,
                    TransferType.Common.ToString()
                    );

                return Ok(new OffchainTradeRespModel
                {
                    TransferId = response.TransferId,
                    TransactionHex = response.TransactionHex,
                    OperationResult = response.OperationResult
                });
            }
            catch (OffchainException ex)
            {
                return NotFound(ProcessError(ex)); 
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
            catch (OffchainException ex)
            {
                return NotFound(ProcessError(ex));
            }
        }

        [HttpPost("finalizetransfer")]
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

            try
            {
                var response = await _offchainService.Finalize(clientId, model.TransferId, model.ClientRevokePubKey,
                    model.ClientRevokeEncryptedPrivateKey, model.SignedTransferTransaction);

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
            catch (OffchainException ex)
            {
                return NotFound(ProcessError(ex));
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


        private ErrorResponse ProcessError(OffchainException ex)
        {
            return new ErrorResponse(ex.OffchainExceptionMessage, ex.OffchainExceptionCode);
            //switch (ex.Type)
            //{
            //    case ErrorCode.LowVolume:
            //        return new ErrorResponse(ex.Message, ErrorCode.LowVolume.ToString());//( ResponseModel<T>.CreateFail(ResponseModel.ErrorCodeType.InvalidInputField, Phrases.LowTradeVolume);
            //    case ErrorCode.NoCoinsToRefund: // need to skip this
            //        return new ErrorResponse(ex.OffchainExceptionMessage, ex.OffchainExceptionCode);
            //    case ErrorCode.NotEnoughAssetAvailable:
            //    case ErrorCode.NotEnoughBitcoinAvailable:
            //        return new ErrorResponse(ex.OffchainExceptionMessage, ex.OffchainExceptionCode);
            //    case ErrorCode.NotEnoughtClientFunds:
            //        return new ErrorResponse(ex.OffchainExceptionMessage, ex.OffchainExceptionCode);
            //    case ErrorCode.ChannelWasBroadcasted:
            //    case ErrorCode.DuplicateRequest:
            //        return new ErrorResponse(ex.OffchainExceptionMessage, ex.OffchainExceptionCode);
            //    case ErrorCode.ChannelNotFinalized:
            //        return new ErrorResponse(ex.OffchainExceptionMessage, ex.OffchainExceptionCode);
            //    case ErrorCode.KeyUsedAlready:
            //        return new ErrorResponse(ex.OffchainExceptionMessage, ex.OffchainExceptionCode);
            //    default:
            //        return new ErrorResponse(ex.OffchainExceptionMessage, ex.OffchainExceptionCode);
            //}
        }
    }
}
