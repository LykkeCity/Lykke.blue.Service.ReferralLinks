// ReSharper disable ClassNeverInstantiated.Global
using Common;
using Common.Log;
using Lykke.Bitcoin.Api.Client.BitcoinApi;
using Lykke.Bitcoin.Api.Client.BitcoinApi.Models;
using Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain;
using Lykke.blue.Service.ReferralLinks.Core.Domain.WalletCredentials;
using Lykke.blue.Service.ReferralLinks.Core.Services;
using System;
using System.Threading.Tasks;

//OffchainService and all offchain functionality will be removed in next PR. No need to review.

namespace Lykke.blue.Service.ReferralLinks.Services.Offchain
{
    public class OffchainService : IOffchainService
    {
        private readonly IBitcoinApiClient _bitcoinApiClient;
        private readonly IWalletCredentialsRepository _walletCredentialsRepository;
        private readonly IOffchainTransferRepository _offchainTransferRepository;
        private readonly IOffchainEncryptedKeysRepository _offchainEncryptedKeysRepository;
        private readonly ILog _logger;
        private readonly IOffchainFinalizeCommandProducer _offchainFinalizeCommandProducer;

        public OffchainService(IBitcoinApiClient bitcoinApiClient, 
                                IWalletCredentialsRepository walletCredentialsRepository, 
                                IOffchainTransferRepository offchainTransferRepository,
                                IOffchainEncryptedKeysRepository offchainEncryptedKeysRepository,
                                IOffchainFinalizeCommandProducer offchainFinalizeCommandProducer,
                                ILog logger)
        {
            _bitcoinApiClient = bitcoinApiClient;
            _walletCredentialsRepository = walletCredentialsRepository;
            _offchainTransferRepository = offchainTransferRepository;
            _offchainEncryptedKeysRepository = offchainEncryptedKeysRepository;
            _offchainFinalizeCommandProducer = offchainFinalizeCommandProducer;
            _logger = logger;
        }

        public async Task<OffchainResult> CreateDirectTransfer(string clientId, string assetId, decimal amount, string prevTempPrivateKey)
        {
            var credentials = await _walletCredentialsRepository.GetAsync(clientId);

            var request = new { clientId, assetId, amount, prevTempPrivateKey };

            await _logger.WriteInfoAsync("CreateDirectTransfer", request.ToJson(), "Transfer requested");

            var offchainTransferResult = await _bitcoinApiClient.OffchainTransferAsync(new OffchainTransferData
            {
                Amount = -amount,
                AssetId = assetId,
                ClientPrevPrivateKey = prevTempPrivateKey,
                ClientPubKey = credentials.PublicKey,
                Required = false
            });

            var createTransferResult = await _offchainTransferRepository.CreateTransfer(Guid.NewGuid().ToString(), clientId, assetId, amount, OffchainTransferType.DirectTransferFromClient, offchainTransferResult.TransferId?.ToString());

            if (offchainTransferResult.HasError)
                return await InternalErrorProcessing("CreateTransfer", offchainTransferResult.Error, credentials, createTransferResult, false);

            var result = new OffchainResult
            {
                TransferId = createTransferResult.Id,
                TransactionHex = offchainTransferResult.Transaction,
                OperationResult = OffchainOperationResult.Transfer
            };

            await _logger.WriteInfoAsync("CreateDirectTransfer", request.ToJson(), new { result.TransferId, result.OperationResult }.ToJson());

            return result;

        }

        private async Task<OffchainResult> InternalErrorProcessing(string process, ErrorResponse error, IWalletCredentials credentials, IOffchainTransfer offchainTransfer, bool required)
        {
            if (error.ErrorCode == ErrorCode.ShouldOpenNewChannel)
                return await CreateChannel(credentials, offchainTransfer, required);

            var offchainTransferInfo = (new {
                offchainTransfer.ClientId, Asset = offchainTransfer.AssetId,
                offchainTransfer.Amount,
                offchainTransfer.Type }).ToJson();

            await _logger.WriteErrorAsync(process, offchainTransferInfo, new Exception($"{error.Message}, Code: {error.Code}"));

            throw new OffchainException(error.ErrorCode, error.Message, error.Code, offchainTransfer.AssetId);
        }

        private async Task<OffchainResult> CreateChannel(IWalletCredentials credentials, IOffchainTransfer offchainTransfer, bool required)
        {
            if (offchainTransfer == null || offchainTransfer.ClientId != credentials.ClientId || offchainTransfer.Completed)
                throw new OffchainException(ErrorCode.Exception, offchainTransfer?.AssetId);

            var fromClient = offchainTransfer.Type == OffchainTransferType.DirectTransferFromClient;

            var result = await _bitcoinApiClient.CreateChannelAsync(new CreateChannelData
            {
                AssetId = offchainTransfer.AssetId,
                ClientPubKey = credentials.PublicKey,
                ClientAmount = fromClient ? offchainTransfer.Amount : 0,
                HubAmount =  0,
                Required = required,
                ExternalTransferId = offchainTransfer.ExternalTransferId
            });

            var offchainTransferInfo = (new {
                offchainTransfer.ClientId, Asset = offchainTransfer.AssetId,
                offchainTransfer.Amount,
                offchainTransfer.Type }).ToJson();

            if (!result.HasError)
            {
                await _offchainTransferRepository.UpdateTransfer(offchainTransfer.Id, result.TransferId?.ToString(), closing: result.ChannelClosing, onchain: true);

                var offchainResult = new OffchainResult
                {
                    TransferId = offchainTransfer.Id,
                    TransactionHex = result.Transaction,
                    OperationResult = result.ChannelClosing ? OffchainOperationResult.Transfer : OffchainOperationResult.CreateChannel
                };

                await _logger.WriteInfoAsync("CreateChannel", offchainTransferInfo, $"Offchain channel successfully created: {(new { offchainResult.TransferId, offchainResult.OperationResult }).ToJson()}");

                return offchainResult;               
            }

            await _logger.WriteErrorAsync("CreateChannel", offchainTransferInfo, new Exception($"{result.Error.Message}, Code: {result.Error.Code} "));

            throw new OffchainException(result.Error.ErrorCode, result.Error.Message, result.Error.Code, offchainTransfer.AssetId);
        }

        public async Task<OffchainResult> CreateHubCommitment(string clientId, string transferId, string signedChannel)
        {
            var credentials = await _walletCredentialsRepository.GetAsync(clientId);
            var offchainTransfer = await _offchainTransferRepository.GetTransfer(transferId);

            if (offchainTransfer.Completed)
            {
                await _logger.WriteErrorAsync("CreateHubCommitment", (new { ClientId = clientId, TransferId = transferId }).ToJson() + " Offchain transfer already completed!", new OffchainException(ErrorCode.Exception, offchainTransfer.AssetId));
                throw new OffchainException(ErrorCode.Exception, offchainTransfer.AssetId);
            }


            if (offchainTransfer.ClientId != clientId)
            {
                await _logger.WriteErrorAsync("CreateHubCommitment", (new { ClientId = clientId, TransferId = transferId }).ToJson() + $" Offchain transfer set for a different client: offchainTransfer.ClientId=={offchainTransfer.ClientId}!", new OffchainException(ErrorCode.Exception, offchainTransfer.AssetId));
                throw new OffchainException(ErrorCode.Exception, offchainTransfer.AssetId);
            }                

            var amount = 0.0M;

            switch (offchainTransfer.Type)
            {
                case OffchainTransferType.DirectTransferFromClient:
                    amount = -offchainTransfer.Amount;
                    break;
                default: throw new OffchainException(ErrorCode.OperationNotSupported, $"Unsuported offchainTransfer.Type: {offchainTransfer.Type} while initializing CreateHubCommitment.", "", "", false  );
            }

            var result = await _bitcoinApiClient.CreateHubCommitment(new CreateHubComitmentData
            {
                Amount = amount,
                ClientPubKey = credentials.PublicKey,
                AssetId = offchainTransfer.AssetId,
                SignedByClientChannel = signedChannel
            });

            if (result.HasError)
                return await InternalErrorProcessing("ProcessClientTransfer", result.Error, credentials, offchainTransfer, required:false);

            return new OffchainResult
            {
                TransferId = offchainTransfer.Id,
                TransactionHex = result.Transaction,
                OperationResult = OffchainOperationResult.Transfer
            };
        }

        public async Task<OffchainResult> Finalize(string clientId, string transferId, string clientRevokePubKey, string clientRevokeEncryptedPrivateKey, string signedCommitment)
        {
            var credentials = await _walletCredentialsRepository.GetAsync(clientId);
            var offchainTransfer = await _offchainTransferRepository.GetTransfer(transferId);

            if (offchainTransfer.Completed)
            {
                await _logger.WriteErrorAsync("CreateHubCommitment", (new { ClientId = clientId, TransferId = transferId }).ToJson() + " Offchain transfer already completed!", new OffchainException(ErrorCode.Exception, offchainTransfer.AssetId));
                throw new OffchainException(ErrorCode.Exception, offchainTransfer.AssetId);
            }


            if (offchainTransfer.ClientId != clientId)
            {
                await _logger.WriteErrorAsync("CreateHubCommitment", (new { ClientId = clientId, TransferId = transferId }).ToJson() + $" Offchain transfer set for a different client: offchainTransfer.ClientId=={offchainTransfer.ClientId}!", new OffchainException(ErrorCode.Exception, offchainTransfer.AssetId));
                throw new OffchainException(ErrorCode.Exception, offchainTransfer.AssetId);
            }

            switch (offchainTransfer.Type)
            {
                case OffchainTransferType.DirectTransferFromClient:
                    return await ProcessTransfer(credentials, offchainTransfer, clientRevokePubKey, clientRevokeEncryptedPrivateKey, signedCommitment);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private async Task<OffchainResult> ProcessTransfer(IWalletCredentials credentials, IOffchainTransfer offchainTransfer, string revokePubKey, string encryptedRevokePrivakeKey,
            string signedCommitment)
        {
            var result = await _bitcoinApiClient.Finalize(new FinalizeData
            {
                ClientPubKey = credentials.PublicKey,
                AssetId = offchainTransfer.AssetId,
                ClientRevokePubKey = revokePubKey,
                SignedByClientHubCommitment = signedCommitment,
                ExternalTransferId = offchainTransfer.ExternalTransferId,
                OffchainTransferId = offchainTransfer.Id
            });

            await _offchainEncryptedKeysRepository.UpdateKey(credentials.ClientId, offchainTransfer.AssetId, encryptedRevokePrivakeKey);

            if (result.HasError)
            {
                return await InternalErrorProcessing(nameof(ProcessTransfer), result.Error, credentials, offchainTransfer, required:false);
            }

            await _offchainTransferRepository.CompleteTransfer(offchainTransfer.Id, blockchainHash: result.TxHash);

            await _offchainFinalizeCommandProducer.ProduceFinalize(offchainTransfer.Id, credentials.ClientId, result.TxHash);

            await _logger.WriteInfoAsync("ProcessTransfer", "Offchain Finalize", $"FinalizeResult: {result.ToJson()}");

            return new OffchainResult
            {
                TransferId = offchainTransfer.Id,
                TransactionHex = result.Transaction,
                OperationResult = OffchainOperationResult.ClientCommitment
            };
        }

    }
}
