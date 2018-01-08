// ReSharper disable ClassNeverInstantiated.Global
using Common;
using Common.Log;
using Lykke.blue.Service.ReferralLinks.Core;
using Lykke.blue.Service.ReferralLinks.Core.Domain.ExchangeOperations;
using Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain;
using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using Lykke.blue.Service.ReferralLinks.Core.Settings;
using Lykke.MatchingEngine.Connector.Abstractions.Models;
using Lykke.Service.ExchangeOperations.Client;
using Lykke.Service.ExchangeOperations.Client.AutorestClient.Models;
using System;
using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.Services.ExchangeOperations
{
    public class ExchangeService
    {
        private readonly ILog _log;
        private readonly IExchangeOperationsServiceClient _exchangeOperationsService;
        private readonly CachedDataDictionary<string, Lykke.Service.Assets.Client.Models.Asset> _assets;
        private readonly AppSettings _settings;

        public ExchangeService(ILog log, 
            IExchangeOperationsServiceClient exchangeOperationsService, 
            CachedDataDictionary<string, Lykke.Service.Assets.Client.Models.Asset> assets,
            AppSettings settings)
        {
            _log = log;
            _exchangeOperationsService = exchangeOperationsService;
            _assets = assets;
            _settings = settings;
        }

        public async Task<ExchangeOperationResult> TransferToSharedWallet(string sourceClientId, double amount, string assetId, string executionContext = null)
        {
            return await ExchangeTransfer(sourceClientId, _settings.ReferralLinksService.LykkeReferralClientId, amount, assetId, executionContext);
        }

        public async Task<ExchangeOperationResult> TransferFromSharedWallet(IReferralLink refLink, string recipientClientId, string executionContext = null)
        {
            return await ExchangeTransfer(_settings.ReferralLinksService.LykkeReferralClientId, recipientClientId, refLink.Amount, refLink.Asset, executionContext);
        }

        private async Task<ExchangeOperationResult> ExchangeTransfer(string sourceClientId, string destClientId, double amount, string assetId, string executionContext = null)
        {
            var request = new { SourceClientId = sourceClientId, DestClientId = destClientId, Amount = amount, AssetId = assetId };

            try
            {
                var result = await _exchangeOperationsService.TransferAsync(
                    destClientId,
                    sourceClientId,
                    amount,
                    assetId,
                    TransferType.Common.ToString()
                );

                if (!result.IsOk())
                {
                    string error = $"Transfer failed: {result.Message}, TxId: {result.TransactionId}";
                    if (result.Code.HasValue && Enum.IsDefined(typeof(MeStatusCodes), result.Code.Value))
                    {
                        error = error + $", ErrorCode: {((MeStatusCodes)result.Code.Value).ToString()}";
                    }
                    else
                    {
                        error = error + $", ErrorCode: {result.Code}";
                    }

                    await _log.WriteErrorAsync(Constants.ComponentName, executionContext, request.ToJson(), new Exception(error));
                    result.Message = error;
                }
                else
                {
                    await _log.WriteInfoAsync(executionContext, request.ToJson(), $"Transfer successfull: {result.ToJson()}");
                }
                return result;


            }
            catch (OffchainException ex)
            {
                throw new Exception($"ExchangeOperationsService error: Code={ex.OffchainExceptionCode}.OffchainException={ex.OffchainExceptionMessage}.Message={ex.Message}.{ex.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"ExchangeOperationsService error: {ex.Message}{ex.InnerException?.Message}");
            }
        }

       

    }
}
