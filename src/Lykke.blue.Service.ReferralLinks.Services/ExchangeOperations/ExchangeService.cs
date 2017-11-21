using Common;
using Common.Log;
using Lykke.blue.Service.ReferralLinks.Core.BitCoinApi.Models;
using Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain;
using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using Lykke.blue.Service.ReferralLinks.Core.Settings.ServiceSettings;
using Lykke.Service.ExchangeOperations.Client;
using Lykke.Service.ExchangeOperations.Client.AutorestClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                var asset = (await _assets.GetDictionaryAsync()).Values.Where(v => v.Symbol == refLink.Asset).FirstOrDefault();
                if (asset == null)
                {
                    var message = $"Asset with symbol {refLink.Asset} not found";
                    await _log.WriteErrorAsync(executionContext, nameof(TransferRewardCoins), (new { Error = message }).ToJson(), new Exception(message), DateTime.Now);
                    return new ExchangeOperationResult { Message = message };
                }

                var result = await _exchangeOperationsService.TransferAsync(
                         recipientClientId,
                         _settings.LykkeReferralClientId,
                         (double)refLink.Amount,
                         asset.Id,
                         TransferType.Common.ToString()
                         );

                if (!result.IsOk())
                {
                    await _log.WriteErrorAsync(executionContext, nameof(TransferRewardCoins), (new { Error = $"TransferAsync from exchangeOperationsService returned error: Message: {result.Message}, Code: {result.Code}" }).ToJson(), new Exception(result.Message), DateTime.Now);
                }

                await _log.WriteInfoAsync(executionContext, request.ToJson(), $"Transfer successfull: {result.ToJson()}");

                return result;
            }
            catch (OffchainException ex)
            {
                await LogClaimReferralLinkError(request, executionContext, ex);
                return new ExchangeOperationResult { };
            }
            catch (ApplicationException ex)
            {
                await LogClaimReferralLinkError(request, executionContext, ex);
                return new ExchangeOperationResult { };
            }
            catch (Exception ex)
            {
                await LogClaimReferralLinkError(request, executionContext, ex);
                return new ExchangeOperationResult { };
            }
        }

        private async Task LogClaimReferralLinkError(dynamic request, string executionContext, Exception ex)
        {
            await _log.WriteErrorAsync(executionContext, request.ToJson(), ex, DateTime.Now);
        }
    }
}
