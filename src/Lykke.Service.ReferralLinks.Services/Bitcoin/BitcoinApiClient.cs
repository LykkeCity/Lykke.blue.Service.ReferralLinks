using Lykke.Blue.Service.ReferralLinks.Core.BitCoinApi;
using Lykke.Blue.Service.ReferralLinks.Core.Settings.ServiceSettings;
using System;
using System.Collections.Generic;
using System.Text;
using Core.BitCoin.BitcoinApi.Models;
using System.Threading.Tasks;
using Lykke.Service.BitcoinApi.Client.Models;
using Lykke.Service.BitcoinApi.Client;

namespace Lykke.Blue.Service.ReferralLinks.Services.Bitcoin
{
    public class BitcoinApiClient : IBitcoinApiClient
    {
        private readonly Lykke.Service.BitcoinApi.Client.BitcoinApi _apiClient;

        public BitcoinApiClient(ReferralLinksSettings settings)
        {
            _apiClient = new Lykke.Service.BitcoinApi.Client.BitcoinApi(new Uri(settings.ExternalServices.BitcoinCoreApiUrl));
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
            var request = new TransferModel(data.ClientPubKey, (decimal?)data.Amount, data.AssetId, data.ClientPrevPrivateKey, data.Required, !string.IsNullOrWhiteSpace(data.ExternalTransferId) ? Guid.Parse(data.ExternalTransferId) : (Guid?)null);

            var response = await _apiClient.ApiOffchainTransferPostAsync(request);

            return PrepareOffchainResult(response);
        }

        public async Task<OffchainResponse> CreateHubCommitment(CreateHubComitmentData data)
        {
            var request = new CreateHubCommitmentModel(data.ClientPubKey, data.AssetId, data.Amount, data.SignedByClientChannel);

            var response = await _apiClient.ApiOffchainCreatehubcommitmentPostAsync(request);

            return PrepareOffchainResult(response);
        }

        public async Task<OffchainBaseResponse> CloseChannel(CloseChannelData data)
        {
            var request = new BroadcastClosingChannelModel(data.ClientPubKey, data.AssetId, data.SignedClosingTransaction, !string.IsNullOrWhiteSpace(data.OffchainTransferId) ? Guid.Parse(data.OffchainTransferId) : (Guid?)null);

            var response = await _apiClient.ApiOffchainBroadcastclosingPostAsync(request);

            return PrepareOffchainTransactionHashResult(response);
        }

        public async Task<OffchainResponse> Finalize(FinalizeData data)
        {
            var request = new FinalizeChannelModel(data.ClientPubKey, data.AssetId, data.ClientRevokePubKey, data.SignedByClientHubCommitment, !string.IsNullOrWhiteSpace(data.ExternalTransferId) ? Guid.Parse(data.ExternalTransferId) : (Guid?)null, !string.IsNullOrWhiteSpace(data.OffchainTransferId) ? Guid.Parse(data.OffchainTransferId) : (Guid?)null);

            var response = await _apiClient.ApiOffchainFinalizePostAsync(request);

            return PrepareFinalizeOffchainResult(response);
        }

        private OffchainResponse PrepareFinalizeOffchainResult(object response)
        {
            var error = response as ApiException;

            if (error != null)
            {
                return new OffchainResponse
                {
                    Error = new ErrorResponse (error.Error.Message, error.Error.Code )
                };
            }

            var transaction = response as FinalizeOffchainApiResponse;
            if (transaction != null)
            {
                return new OffchainResponse
                {
                    Transaction = transaction.Transaction,
                    TransferId = transaction.TransferId,
                    TxHash = transaction.Hash
                };
            }

            throw new ArgumentException("Unkown response object");
        }

        private OffchainBaseResponse PrepareOffchainTransactionHashResult(object response)
        {
            var error = response as ApiException;

            if (error != null)
            {
                return new OffchainBaseResponse
                {
                    Error = new ErrorResponse ( error.Error.Message, error.Error.Code /* Code = error.Error.Code, Message = error.Error.Message*/ )
                };
            }

            var transaction = response as TransactionHashResponse;
            if (transaction != null)
            {
                return new OffchainBaseResponse
                {
                    TxHash = transaction.TransactionHash
                };
            }

            throw new ArgumentException("Unkown response object");
        }

        private OffchainClosingResponse PrepareOffchainClosingResult(object response)
        {
            var error = response as ApiException;

            if (error != null)
            {
                return new OffchainClosingResponse
                {
                    Error = new ErrorResponse(error.Error.Message, error.Error.Code) //{ Code = error.Error.Code, Message =  }
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
                    Error = new ErrorResponse (error.Error.Message, error.Error.Code) 
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
