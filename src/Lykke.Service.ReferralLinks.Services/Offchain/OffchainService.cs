using Lykke.Service.ReferralLinks.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Lykke.Service.ReferralLinks.Core.Domain.Offchain;
using System.Threading.Tasks;
using Lykke.Service.ReferralLinks.Core.BitCoinApi;
using Lykke.Service.ReferralLinks.AzureRepositories.WalletCredentials;
using Core.BitCoin.BitcoinApi.Models;
using Lykke.Service.ReferralLinks.Core.Domain.WalletCredentials;
using Common.Log;

namespace Lykke.Service.ReferralLinks.Services.Offchain
{
    public class OffchainService : IOffchainService
    {
        private readonly IBitcoinApiClient _bitcoinApiClient;
        private readonly IWalletCredentialsRepository _walletCredentialsRepository;
        private readonly IOffchainTransferRepository _offchainTransferRepository;
        private readonly ILog _logger;

        public OffchainService(IBitcoinApiClient bitcoinApiClient, 
                                IWalletCredentialsRepository walletCredentialsRepository, 
                                IOffchainTransferRepository offchainTransferRepository, 
                                ILog logger)
        {
            _bitcoinApiClient = bitcoinApiClient;
            _walletCredentialsRepository = walletCredentialsRepository;
            _offchainTransferRepository = offchainTransferRepository;
            _logger = logger;
        }

        public async Task<OffchainResult> CreateDirectTransfer(string clientId, string asset, decimal amount, string prevTempPrivateKey)
        {
            var credentials = await _walletCredentialsRepository.GetAsync(clientId);

            var result = await _bitcoinApiClient.OffchainTransferAsync(new OffchainTransferData
            {
                Amount = -amount,
                AssetId = asset,
                ClientPrevPrivateKey = prevTempPrivateKey,
                ClientPubKey = credentials.PublicKey,
                Required = false
            });

            var offchainTransfer = await _offchainTransferRepository.CreateTransfer(Guid.NewGuid().ToString(), clientId, asset, amount, OffchainTransferType.DirectTransferFromClient, result.TransferId?.ToString(), null);

            if (result.HasError)
                return await InternalErrorProcessing("CreateTransfer", result.Error, credentials, offchainTransfer, false);

            return new OffchainResult
            {
                TransferId = offchainTransfer.Id,
                TransactionHex = result.Transaction,
                OperationResult = OffchainOperationResult.Transfer
            };

        }

        private async Task<OffchainResult> InternalErrorProcessing(string component, ErrorResponse error, IWalletCredentials credentials, IOffchainTransfer offchainTransfer, bool required)
        {
            if (error.ErrorCode == ErrorCode.ShouldOpenNewChannel)
                return await CreateChannel(credentials, offchainTransfer, required);

            await _logger.WriteErrorAsync("OffchainService", component, $"Client: [{credentials.ClientId}], error: [{error.ErrorCode}], transfer: [{offchainTransfer.Id}]", new Exception(error.Message));

            throw new OffchainException(error.ErrorCode, offchainTransfer.AssetId);
        }

        public async Task<OffchainResult> CreateChannel(IWalletCredentials credentials, IOffchainTransfer offchainTransfer, bool required)
        {
            if (offchainTransfer == null || offchainTransfer.ClientId != credentials.ClientId || offchainTransfer.Completed)
                throw new OffchainException(ErrorCode.Exception, offchainTransfer?.AssetId);

            var fromClient = offchainTransfer.Type == OffchainTransferType.FromClient ||
                             offchainTransfer.Type == OffchainTransferType.DirectTransferFromClient ||
                             offchainTransfer.Type == OffchainTransferType.OffchainCashout;

            var fromHub = offchainTransfer.Type == OffchainTransferType.FromHub ||
                          offchainTransfer.Type == OffchainTransferType.CashinToClient;

            var result = await _bitcoinApiClient.CreateChannelAsync(new CreateChannelData
            {
                AssetId = offchainTransfer.AssetId,
                ClientPubKey = credentials.PublicKey,
                ClientAmount = fromClient ? offchainTransfer.Amount : 0,
                HubAmount = fromHub ? offchainTransfer.Amount : 0,
                Required = required,
                ExternalTransferId = offchainTransfer.ExternalTransferId
            });

            if (!result.HasError)
            {
                await _offchainTransferRepository.UpdateTransfer(offchainTransfer.Id, result.TransferId?.ToString(), closing: result.ChannelClosing, onchain: true);

                return new OffchainResult
                {
                    TransferId = offchainTransfer.Id,
                    TransactionHex = result.Transaction,
                    OperationResult = result.ChannelClosing ? OffchainOperationResult.Transfer : OffchainOperationResult.CreateChannel
                };
            }

            await _logger.WriteErrorAsync("OffchainService", "CreateChannel", $"Client: [{credentials.ClientId}], error: [{result.Error.ErrorCode}], transfer: [{offchainTransfer.Id}]", new Exception(result.Error.Message));

            throw new OffchainException(result.Error.ErrorCode, offchainTransfer.AssetId);
        }
    }
}
