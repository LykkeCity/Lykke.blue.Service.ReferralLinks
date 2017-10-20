using Lykke.Service.ReferralLinks.Core.Domain.Client;
using Lykke.Service.ReferralLinks.Core.Kyc;
using Lykke.Service.ReferralLinks.Models.Offchain;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.ReferralLinks.Controllers
{
    [Route("api/transfers")]
    public class TransfersController : Controller
    {
        private readonly ISrvKycForAsset _srvKycForAsset;
        private readonly IClientSettingsRepository _clientSettingsRepository;

        

        public TransfersController(ISrvKycForAsset srvKycForAsset, IClientSettingsRepository clientSettingsRepository)
        {
            _srvKycForAsset = srvKycForAsset;
            _clientSettingsRepository = clientSettingsRepository;
        }

        protected async Task CheckOffchain(string clientId)
        {
            if (!await _clientSettingsRepository.IsOffchainClient(clientId))
                throw new Exception("Offchain is not supported");
        }

        [HttpPost("toLykke")]
        public async Task<IActionResult> TransferToLykke([FromBody] OffchainTransferToLykkeModel model)
        {
            var clientId = model.ClientId;

            await CheckOffchain(clientId);

            //var marginDataReader = _marginDataServiceResolver.GetDataReader(false);
            //var accounts = await marginDataReader.GetAccountsByClientIdAsync(clientId);
            //var account = accounts.SingleOrDefault(a => a.Id == model.AccountId);

            //if (account == null)
            //    return ResponseModel<OffchainTradeRespModel>.CreateFail(ResponseModel.ErrorCodeType.NoData, Phrases.InvalidValue);

            //if (await _srvKycForAsset.IsKycNeeded(clientId, model.Asset))
            //    return ResponseModel<OffchainTradeRespModel>.CreateFail(ResponseModel.ErrorCodeType.InconsistentData, Phrases.KycNeeded);

            //var accountGroup = (await marginDataReader.GetAccountGroupWithHttpMessagesAsync(account.TradingConditionId, account.BaseAssetId)).Body;

            //if (accountGroup == null)
            //    return ResponseModel<OffchainTradeRespModel>.CreateFail(ResponseModel.ErrorCodeType.NoData, Phrases.InvalidValue);

            //if (accountGroup.DepositTransferLimit > 0 && accountGroup.DepositTransferLimit <
            //    account.Balance + Math.Abs((double)model.Amount))
            //    return ResponseModel<OffchainTradeRespModel>.CreateFail(ResponseModel.ErrorCodeType.InconsistentData,
            //        string.Format(Phrases.MaxMarginTransferLimitExceeded,
            //            accountGroup.DepositTransferLimit, account.BaseAssetId));

            //try
            //{
            //    var response = await _offchainService.CreateDirectTransfer(clientId, account.BaseAssetId, model.Amount, model.PrevTempPrivateKey);

            //    var additionalActionsSrc = new TransferContextData.AdditionalActions
            //    {
            //        UpdateMarginBalance = new TransferContextData.UpdateMarginBalance(model.AccountId, (double)model.Amount)
            //    };

            //    await _exchangeOperationsService.StartTransferAsync(
            //        response.TransferId,
            //        _baseSettings.MarginSettings.MoneyTrasferClientId,
            //        clientId,
            //        TransferType.ToMarginAccount.ToString(),
            //        additionalActionsSrcJson: additionalActionsSrc.ToJson());

            //    return ResponseModel<OffchainTradeRespModel>.CreateOk(new OffchainTradeRespModel
            //    {
            //        TransferId = response.TransferId,
            //        TransactionHex = response.TransactionHex,
            //        OperationResult = response.OperationResult
            //    });
            //}
            //catch (OffchainException ex)
            //{
            //    return await ProcessError<OffchainTradeRespModel>(ex);
            //}

            return null;
        }
    }
}
