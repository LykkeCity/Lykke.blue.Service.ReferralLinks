using Common;
using Common.Log;
using Lykke.blue.Service.ReferralLinks.Core.Domain.ExchangeOperations;
using Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain;
using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using Lykke.blue.Service.ReferralLinks.Core.Settings.ServiceSettings;
using Lykke.MatchingEngine.Connector.Abstractions.Models;
using Lykke.Service.ExchangeOperations.Client;
using Lykke.Service.ExchangeOperations.Client.AutorestClient.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.Services.ExchangeOperations
{
    public class ExchangeService
    {
        private readonly ILog _log;
        private readonly IExchangeOperationsServiceClient _exchangeOperationsService;
        private readonly CachedDataDictionary<string, Lykke.Service.Assets.Client.Models.Asset> _assets;
        private readonly ReferralLinksSettings _settings;

        public ExchangeService(ILog log, 
            IExchangeOperationsServiceClient exchangeOperationsService, 
            CachedDataDictionary<string, Lykke.Service.Assets.Client.Models.Asset> assets,
            ReferralLinksSettings settings)
        {
            _log = log;
            _exchangeOperationsService = exchangeOperationsService;
            _assets = assets;
            _settings = settings;
        }

        public async Task<ExchangeOperationResult> TransferRewardCoins(IReferralLink refLink, bool isNewClient, string recipientClientId, string executionContext)
        {
            var request = new { RefLinkId = refLink.Id, RefLinkUrl = refLink.Url, RecipientClientId = recipientClientId, IsNewClient = isNewClient };

            try
            {
                var asset = (await _assets.GetDictionaryAsync()).Values.FirstOrDefault(v => v.Symbol == refLink.Asset);
                if (asset == null)
                {
                    var message = $"Asset with symbol {refLink.Asset} not found";
                    await _log.WriteErrorAsync(executionContext, nameof(TransferRewardCoins), (new { Error = message }).ToJson(), new Exception(message), DateTime.Now);
                    return new ExchangeOperationResult { Message = message };
                }

                var result = await _exchangeOperationsService.TransferAsync(
                         recipientClientId,
                         _settings.LykkeReferralClientId,
                         refLink.Amount,
                         asset.Id,
                         TransferType.Common.ToString()
                         );

                if (!result.IsOk())
                {
                    string error;
                    if(result.Code.HasValue && Enum.IsDefined(typeof(MeStatusCodes), result.Code.Value))
                    {
                        error = $"Error: {((MeStatusCodes)result.Code.Value).ToString()}, Message: {result.Message}, TransactionId: {result.TransactionId}";                        
                    }
                    else
                    {
                        error = $"Error: {result.Code}, Message: {result.Message}, TransactionId: {result.TransactionId}";
                    }
                    await _log.WriteErrorAsync(executionContext, nameof(TransferRewardCoins), error, new Exception(error), DateTime.Now);
                    result.Message = error;
                    return result;
                }
                await _log.WriteInfoAsync(executionContext, request.ToJson(), $"Transfer successfull: {result.ToJson()}");
                return result;
            }
            catch (OffchainException ex)
            {
                await _log.WriteErrorAsync(executionContext, request.ToJson(), ex, DateTime.Now);
                throw new Exception($"ExchangeOperationsService error: Code={ex.OffchainExceptionCode}.OffchainException={ex.OffchainExceptionMessage}.Message={ex.Message}.{ex.InnerException?.Message}");
            }
            catch (ApplicationException ex)
            {
                await _log.WriteErrorAsync(executionContext, request.ToJson(), ex, DateTime.Now);
                throw new Exception($"ExchangeOperationsService error: {ex.Message}{ex.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(executionContext, request.ToJson(), ex, DateTime.Now);
                throw new Exception($"ExchangeOperationsService error: {ex.Message}{ex.InnerException?.Message}");
            }
        }

    }
}
