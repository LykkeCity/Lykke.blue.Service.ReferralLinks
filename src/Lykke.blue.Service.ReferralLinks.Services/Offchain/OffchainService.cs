using Common;
using Common.Log;
using Lykke.blue.Service.ReferralLinks.Core.BitCoinApi;
using Lykke.blue.Service.ReferralLinks.Core.BitCoinApi.Models;
using Lykke.blue.Service.ReferralLinks.Core.Domain;
using Lykke.blue.Service.ReferralLinks.Core.Domain.Exceptions;
using Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain;
using Lykke.blue.Service.ReferralLinks.Core.Domain.WalletCredentials;
using Lykke.blue.Service.ReferralLinks.Core.Services;
using Lykke.MatchingEngine.Connector.Abstractions.Models;
using Lykke.MatchingEngine.Connector.Abstractions.Services;
using Lykke.Service.Assets.Client.Models;
using System;
using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.Services.Offchain
{
    public class OffchainService : IOffchainService
    {
        private readonly IBitcoinApiClient _bitcoinApiClient;
        private readonly IWalletCredentialsRepository _walletCredentialsRepository;
        private readonly IOffchainTransferRepository _offchainTransferRepository;
        private readonly IOffchainEncryptedKeysRepository _offchainEncryptedKeysRepository;
        private readonly IOffchainOrdersRepository _offchainOrdersRepository;
        private readonly IBitcoinTransactionService _bitcoinTransactionService;
        private readonly IOffchainRequestService _offchainRequestService;
        private readonly ILog _logger;
        private readonly IMatchingEngineClient _matchingEngineConnector;
        private readonly IOffchainFinalizeCommandProducer _offchainFinalizeCommandProducer;
        private readonly CachedDataDictionary<string, AssetPair> _assetPairs;
        private readonly CachedDataDictionary<string, Asset> _assets;

        public OffchainService(IBitcoinApiClient bitcoinApiClient, 
                                IWalletCredentialsRepository walletCredentialsRepository, 
                                IOffchainTransferRepository offchainTransferRepository,
                                IMatchingEngineClient matchingEngineConnector,
                                IOffchainEncryptedKeysRepository offchainEncryptedKeysRepository,
                                IOffchainOrdersRepository offchainOrdersRepository,
                                IBitcoinTransactionService bitcoinTransactionService,
                                IOffchainFinalizeCommandProducer offchainFinalizeCommandProducer,
                                IOffchainRequestService offchainRequestService,
                                CachedDataDictionary<string, AssetPair> assetPairs,
                                CachedDataDictionary<string, Asset> assets,
                                ILog logger)
        {
            _bitcoinApiClient = bitcoinApiClient;
            _walletCredentialsRepository = walletCredentialsRepository;
            _offchainTransferRepository = offchainTransferRepository;
            _offchainEncryptedKeysRepository = offchainEncryptedKeysRepository;
            _offchainOrdersRepository = offchainOrdersRepository;
            _bitcoinTransactionService = bitcoinTransactionService;
            _matchingEngineConnector = matchingEngineConnector;
            _offchainFinalizeCommandProducer = offchainFinalizeCommandProducer;
            _offchainRequestService = offchainRequestService;
            _logger = logger;
            _assetPairs = assetPairs;
            _assets = assets;
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

            var createTransferResult = await _offchainTransferRepository.CreateTransfer(Guid.NewGuid().ToString(), clientId, assetId, amount, OffchainTransferType.DirectTransferFromClient, offchainTransferResult.TransferId?.ToString(), null);

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

        private async Task<OffchainResult> InternalErrorProcessing(string process, Lykke.blue.Service.ReferralLinks.Core.BitCoinApi.Models.ErrorResponse error, IWalletCredentials credentials, IOffchainTransfer offchainTransfer, bool required)
        {
            if (error.ErrorCode == ErrorCode.ShouldOpenNewChannel)
                return await CreateChannel(credentials, offchainTransfer, required);

            var offchainTransferInfo = (new {
                offchainTransfer.ClientId, Asset = offchainTransfer.AssetId,
                offchainTransfer.Amount,
                offchainTransfer.Type }).ToJson();

            await _logger.WriteErrorAsync(process, offchainTransferInfo, new Exception($"{error.Message}, Code: {error.Code}, ErrorCode: {error.ErrorCodeString}"));

            throw new OffchainException(error.ErrorCode, error.Message, error.Code, offchainTransfer.AssetId);
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
                await _logger.WriteErrorAsync("CreateHubCommitment", (new { ClientId = clientId, TransferId = transferId }).ToJson(), new OffchainException(ErrorCode.OffchainTransferAlreadyCompleted, offchainTransfer.AssetId));
                throw new OffchainException(ErrorCode.OffchainTransferAlreadyCompleted, offchainTransfer.AssetId);
            }


            if (offchainTransfer.ClientId != clientId)
            {
                await _logger.WriteErrorAsync("CreateHubCommitment", (new { ClientId = clientId, TransferId = transferId }).ToJson(), new OffchainException(ErrorCode.RequestedClientDoesNotMatchTransferClient, offchainTransfer.AssetId));
                throw new OffchainException(ErrorCode.RequestedClientDoesNotMatchTransferClient, offchainTransfer.AssetId);
            }                

            var amount = 0.0M;
            var required = false;
            switch (offchainTransfer.Type)
            {
                case OffchainTransferType.DirectTransferFromClient:
                case OffchainTransferType.OffchainCashout:
                case OffchainTransferType.FromClient:
                    amount = -offchainTransfer.Amount;
                    break;
                case OffchainTransferType.CashinToClient:
                case OffchainTransferType.FromHub:
                    amount = offchainTransfer.Amount;
                    required = true;
                    break;
            }

            var result = await _bitcoinApiClient.CreateHubCommitment(new CreateHubComitmentData
            {
                Amount = amount,
                ClientPubKey = credentials.PublicKey,
                AssetId = offchainTransfer.AssetId,
                SignedByClientChannel = signedChannel
            });

            if (result.HasError)
                return await InternalErrorProcessing("ProcessClientTransfer", result.Error, credentials, offchainTransfer, required);

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
                await _logger.WriteErrorAsync("CreateHubCommitment", (new { ClientId = clientId, TransferId = transferId }).ToJson(), new OffchainException(ErrorCode.OffchainTransferAlreadyCompleted, offchainTransfer.AssetId));
                throw new OffchainException(ErrorCode.OffchainTransferAlreadyCompleted, offchainTransfer.AssetId);
            }


            if (offchainTransfer.ClientId != clientId)
            {
                await _logger.WriteErrorAsync("CreateHubCommitment", (new { ClientId = clientId, TransferId = transferId }).ToJson(), new OffchainException(ErrorCode.RequestedClientDoesNotMatchTransferClient, offchainTransfer.AssetId));
                throw new OffchainException(ErrorCode.RequestedClientDoesNotMatchTransferClient, offchainTransfer.AssetId);
            }

            switch (offchainTransfer.Type)
            {
                case OffchainTransferType.FromClient:
                    return await ProcessClientTransfer(credentials, offchainTransfer, clientRevokePubKey,
                        clientRevokeEncryptedPrivateKey, signedCommitment);
                case OffchainTransferType.FromHub:
                case OffchainTransferType.CashinFromClient:
                case OffchainTransferType.CashinToClient:
                case OffchainTransferType.DirectTransferFromClient:
                    return await ProcessTransfer(credentials, offchainTransfer, clientRevokePubKey,
                        clientRevokeEncryptedPrivateKey, signedCommitment);
                case OffchainTransferType.OffchainCashout:
                case OffchainTransferType.ClientCashout:
                case OffchainTransferType.HubCashout:
                    {
                        if (offchainTransfer.ChannelClosing)
                            return await ProcessChannelClosing(credentials, offchainTransfer, signedCommitment);
                        return await ProcessCashout(credentials, offchainTransfer, clientRevokePubKey, clientRevokeEncryptedPrivateKey, signedCommitment);
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public async Task<OffchainResultOrder> GetResultOrderFromTransfer(string transferId)
        {
            var offchainTransfer = await _offchainTransferRepository.GetTransfer(transferId);

            if (string.IsNullOrWhiteSpace(offchainTransfer.OrderId))
                return null;

            var offchainOrder = await _offchainOrdersRepository.GetOrder(offchainTransfer.OrderId);

            if (offchainOrder.Price == 0)
                return null;

            var assetPair = await _assetPairs.GetItemAsync(offchainOrder.AssetPair);
            var otherAsset = await _assets.GetItemAsync(assetPair.BaseAssetId == offchainOrder.Asset ? assetPair.QuotingAssetId : assetPair.BaseAssetId);

            var orderBuy = offchainOrder.Volume > 0;

            var price = offchainOrder.Straight ? offchainOrder.Price : 1 / offchainOrder.Price;
            var rate = ((double)price).TruncateDecimalPlaces(offchainOrder.Straight ? assetPair.Accuracy : assetPair.InvertedAccuracy, orderBuy);

            var converted = (rate * (double)offchainOrder.Volume).TruncateDecimalPlaces(otherAsset.Accuracy, orderBuy);

            var totalCost = orderBuy ? converted : (double)offchainOrder.Volume;
            var volume = orderBuy ? (double)offchainOrder.Volume : converted;

            return new OffchainResultOrder
            {
                Asset = offchainOrder.Asset,
                Id = offchainOrder.Id,
                AssetPair = offchainOrder.AssetPair,
                Volume = volume,
                Price = rate,
                TotalCost = totalCost,
                DateTime = offchainOrder.CreatedAt,
                OrderType = offchainOrder.Volume > 0 ? OrderType.Buy : OrderType.Sell
            };
        }

        private async Task<OffchainResult> ProcessCashout(IWalletCredentials credentials, IOffchainTransfer offchainTransfer, string revokePubKey, string encryptedRevokePrivakeKey,
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
                return await InternalErrorProcessing(nameof(ProcessCashout), result.Error, credentials, offchainTransfer, false);

            var isOnchain = offchainTransfer.Type == OffchainTransferType.ClientCashout || offchainTransfer.Type == OffchainTransferType.HubCashout;

            await _offchainTransferRepository.CompleteTransfer(offchainTransfer.Id, isOnchain, blockchainHash: result.TxHash);

            await _offchainFinalizeCommandProducer.ProduceFinalize(offchainTransfer.Id, credentials.ClientId, result.TxHash);

            return new OffchainResult
            {
                TransferId = offchainTransfer.Id,
                TransactionHex = "0x0", //result.Transaction,
                OperationResult = OffchainOperationResult.ClientCommitment
            };
        }



        private async Task<OffchainResult> ProcessChannelClosing(IWalletCredentials credentials, IOffchainTransfer offchainTransfer, string signedTransaction)
        {
            if (offchainTransfer.Completed || offchainTransfer.ClientId != credentials.ClientId)
                throw new OffchainException(ErrorCode.Exception, offchainTransfer.AssetId);

            var result = await _bitcoinApiClient.CloseChannel(new CloseChannelData
            {
                ClientPubKey = credentials.PublicKey,
                AssetId = offchainTransfer.AssetId,
                SignedClosingTransaction = signedTransaction,
                OffchainTransferId = offchainTransfer.Id
            });

            if (result.HasError)
                return await InternalErrorProcessing(nameof(ProcessChannelClosing), result.Error, credentials, offchainTransfer, false);

            await _offchainTransferRepository.CompleteTransfer(offchainTransfer.Id, true, blockchainHash: result.TxHash);

            await _offchainFinalizeCommandProducer.ProduceFinalize(offchainTransfer.Id, credentials.ClientId, result.TxHash);

            return new OffchainResult();
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
                var required = offchainTransfer.Type == OffchainTransferType.FromHub || offchainTransfer.Type == OffchainTransferType.CashinToClient;
                return await InternalErrorProcessing(nameof(ProcessTransfer), result.Error, credentials, offchainTransfer, required);
            }

            await _offchainTransferRepository.CompleteTransfer(offchainTransfer.Id, blockchainHash: result.TxHash);

            await _offchainFinalizeCommandProducer.ProduceFinalize(offchainTransfer.Id, credentials.ClientId, result.TxHash);

            return new OffchainResult
            {
                TransferId = offchainTransfer.Id,
                TransactionHex = "0x0", //result.Transaction,
                OperationResult = OffchainOperationResult.ClientCommitment
            };
        }

        private async Task<OffchainResult> ProcessClientTransfer(IWalletCredentials credentials, IOffchainTransfer offchainTransfer, string revokePubKey, string encryptedRevokePrivakeKey, string signedCommitment)
        {
            OffchainBaseResponse result;

            if (offchainTransfer.ChannelClosing)
            {
                result = await _bitcoinApiClient.CloseChannel(new CloseChannelData
                {
                    ClientPubKey = credentials.PublicKey,
                    AssetId = offchainTransfer.AssetId,
                    SignedClosingTransaction = signedCommitment,
                    OffchainTransferId = offchainTransfer.Id
                });
            }
            else
            {
                result = await _bitcoinApiClient.Finalize(new FinalizeData
                {
                    ClientPubKey = credentials.PublicKey,
                    AssetId = offchainTransfer.AssetId,
                    ClientRevokePubKey = revokePubKey,
                    SignedByClientHubCommitment = signedCommitment,
                    ExternalTransferId = offchainTransfer.ExternalTransferId,
                    OffchainTransferId = offchainTransfer.Id
                });
            }

            await _offchainEncryptedKeysRepository.UpdateKey(credentials.ClientId, offchainTransfer.AssetId, encryptedRevokePrivakeKey);

            if (result.HasError)
                return await InternalErrorProcessing(nameof(ProcessClientTransfer), result.Error, credentials, offchainTransfer, false);

            await _offchainTransferRepository.CompleteTransfer(offchainTransfer.Id, blockchainHash: result.TxHash);

            var order = await _offchainOrdersRepository.GetOrder(offchainTransfer.OrderId);

            // save context for tx handler
            var ctx = new SwapOffchainContextData();
            ctx.Operations.Add(new SwapOffchainContextData.Operation
            {
                Amount = -offchainTransfer.Amount,
                AssetId = offchainTransfer.AssetId,
                ClientId = offchainTransfer.ClientId,
                TransactionId = offchainTransfer.Id
            });
            await _bitcoinTransactionService.SetTransactionContext(offchainTransfer.OrderId, ctx);

            var meOrderAction = order.Volume > 0
                ? OrderAction.Buy
                : OrderAction.Sell;

            MeStatusCodes? status = null;
            try
            {
                if (order.IsLimit)
                {
                    var response = await _matchingEngineConnector.PlaceLimitOrderAsync(order.Id, credentials.ClientId,
                        order.AssetPair, meOrderAction, (double)Math.Abs(order.Volume), (double)order.Price);

                    status = response?.Status;

                    if (status == MeStatusCodes.Ok)
                    {
                        return new OffchainResult
                        {
                            TransferId = offchainTransfer.Id,
                            TransactionHex = "0x0", //result.Transaction,
                            OperationResult = OffchainOperationResult.ClientCommitment
                        };
                    }
                }
                else
                {
                    var response = await _matchingEngineConnector.HandleMarketOrderAsync(order.Id, credentials.ClientId,
                        order.AssetPair, meOrderAction, (double)Math.Abs(order.Volume), order.Straight,
                        (double)offchainTransfer.Amount);

                    status = response?.Status;

                    if (status == MeStatusCodes.Ok)
                    {
                        await _offchainOrdersRepository.UpdatePrice(order.Id, (decimal)response.Price);

                        return new OffchainResult
                        {
                            TransferId = offchainTransfer.Id,
                            TransactionHex = "0x0", //result.Transaction,
                            OperationResult = OffchainOperationResult.ClientCommitment
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                await _logger.WriteErrorAsync("ProcessClientTransfer", $"Client: [{credentials.ClientId}], error: ME failed, order: [{order.Id}], transfer: [{offchainTransfer.Id}]", ex);
            }

            // reverse client transaction if ME returns error
            await _offchainRequestService.CreateOffchainRequest(Guid.NewGuid().ToString(), credentials.ClientId, offchainTransfer.AssetId, offchainTransfer.Amount, order.Id, OffchainTransferType.FromHub);

            await _logger.WriteErrorAsync("ProcessClientTransfer", $"Client: [{credentials.ClientId}], error: ME failed, order: [{order.Id}], transfer: [{offchainTransfer.Id}]", new Exception($"ME failed, order status: {status}"));

            if (status != null)
            {
                if (status == MeStatusCodes.LeadToNegativeSpread)
                    throw new TradeException(TradeExceptionType.LeadToNegativeSpread);
                if (status == MeStatusCodes.NotEnoughFunds)
                    throw new OffchainException(ErrorCode.NotEnoughtClientFunds, offchainTransfer.AssetId);
                if (status == MeStatusCodes.Dust)
                    throw new OffchainException(ErrorCode.LowVolume, offchainTransfer.AssetId);
                if (status == MeStatusCodes.NoLiquidity)
                    throw new OffchainException(offchainTransfer.AssetId == LykkeConstants.BitcoinAssetId ? ErrorCode.NotEnoughBitcoinAvailable : ErrorCode.NotEnoughAssetAvailable, offchainTransfer.AssetId);
            }

            throw new OffchainException(ErrorCode.Exception, offchainTransfer.AssetId);
        }

    }
}
