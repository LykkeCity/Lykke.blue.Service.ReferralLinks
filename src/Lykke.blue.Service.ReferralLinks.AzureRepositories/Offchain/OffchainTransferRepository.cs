using AzureStorage;
using Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain;
using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.AzureRepositories.Offchain
{
    public class OffchainTransferRepository : IOffchainTransferRepository
    {
        private readonly INoSQLTableStorage<OffchainTransferEntity> _storage;

        public OffchainTransferRepository(INoSQLTableStorage<OffchainTransferEntity> storage)
        {
            _storage = storage;
        }

        public async Task<IOffchainTransfer> CreateTransfer(string transactionId, string clientId, string assetId, decimal amount, OffchainTransferType type, string externalTransferId, bool channelClosing = false)
        {
            var entity = OffchainTransferEntity.ByCommon.Create(transactionId, clientId, assetId, amount, type, externalTransferId, channelClosing);

            await _storage.InsertAsync(entity);

            return entity;
        }

        public async Task<IOffchainTransfer> GetTransfer(string id)
        {
            return await _storage.GetDataAsync(OffchainTransferEntity.ByCommon.GeneratePartitionKey(), id);
        }

        public async Task CompleteTransfer(string transferId, bool? onchain = null, string blockchainHash = null)
        {
            await _storage.ReplaceAsync(OffchainTransferEntity.ByCommon.GeneratePartitionKey(), transferId,
                entity =>
                {
                    entity.Completed = true;
                    if (onchain != null)
                        entity.Onchain = onchain.Value;
                    if (!string.IsNullOrWhiteSpace(blockchainHash))
                        entity.BlockchainHash = blockchainHash;
                    return entity;
                });
        }

        public async Task UpdateTransfer(string transferId, string externalTransferId, bool closing = false, bool? onchain = null)
        {
            await _storage.ReplaceAsync(OffchainTransferEntity.ByCommon.GeneratePartitionKey(), transferId,
                 entity =>
                 {
                     entity.ExternalTransferId = externalTransferId;
                     entity.ChannelClosing = closing;
                     if (onchain != null)
                         entity.Onchain = onchain.Value;
                     return entity;
                 });
        }
        
    }
}
