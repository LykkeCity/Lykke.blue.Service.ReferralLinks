using Lykke.Service.ReferralLinks.Core.BitCoinApi;
using Lykke.Service.ReferralLinks.Core.Settings.ServiceSettings;
using System;
using System.Collections.Generic;
using System.Text;
using Core.BitCoin.BitcoinApi.Models;
using System.Threading.Tasks;
using Lykke.Service.BitcoinApi.Client.Models;
using Lykke.Service.BitcoinApi.Client;

namespace Lykke.Service.ReferralLinks.Services.Bitcoin
{
    public class BitcoinApiClient : IBitcoinApiClient
    {
        private readonly Lykke.Service.BitcoinApi.Client.BitcoinApi _apiClient;

        public BitcoinApiClient(BitcoinCoreSettings bitcoinCoreSettings)
        {
            _apiClient = new Lykke.Service.BitcoinApi.Client.BitcoinApi(new Uri(bitcoinCoreSettings.BitcoinCoreApiUrl));
            _apiClient.SetRetryPolicy(null);
        }

        public async Task<OffchainClosingResponse> CreateChannelAsync(CreateChannelData data)
        {
            var request = new CreateChannelModel(data.ClientPubKey, data.HubAmount, data.AssetId, data.Required, !string.IsNullOrWhiteSpace(data.ExternalTransferId) ? Guid.Parse(data.ExternalTransferId) : (Guid?)null, data.ClientAmount);

            var response = await _apiClient.ApiOffchainCreatechannelPostAsync(request);

            return PrepareOffchainClosingResult(response);
        }

        public async Task<OffchainResponse> OffchainTransferAsync(OffchainTransferData data)
        {
            var request = new TransferModel(data.ClientPubKey, data.Amount, data.AssetId, data.ClientPrevPrivateKey, data.Required, !string.IsNullOrWhiteSpace(data.ExternalTransferId) ? Guid.Parse(data.ExternalTransferId) : (Guid?)null);

            var response = await _apiClient.ApiOffchainTransferPostAsync(request);

            return PrepareOffchainResult(response);
        }

        private OffchainClosingResponse PrepareOffchainClosingResult(object response)
        {
            var error = response as ApiException;

            if (error != null)
            {
                return new OffchainClosingResponse
                {
                    Error = new ErrorResponse { Code = error.Error.Code, Message = error.Error.Message }
                };
            }

            var cashout = response as CashoutOffchainApiResponse;
            if (cashout != null)
            {
                return new OffchainClosingResponse
                {
                    Transaction = cashout.Transaction,
                    TransferId = cashout.TransferId,
                    ChannelClosing = cashout.ChannelClosed ?? false
                };
            }

            throw new ArgumentException("Unkown response object");
        }

        private OffchainResponse PrepareOffchainResult(object response)
        {
            var error = response as ApiException;

            if (error != null)
            {
                return new OffchainClosingResponse
                {
                    Error = new ErrorResponse { Code = error.Error.Code, Message = error.Error.Message }
                };
            }

            var transaction = response as OffchainApiResponse;
            if (transaction != null)
            {
                return new OffchainResponse
                {
                    Transaction = transaction.Transaction,
                    TransferId = transaction.TransferId
                };
            }

            throw new ArgumentException("Unkown response object");
        }
    }
}
