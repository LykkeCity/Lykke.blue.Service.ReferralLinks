using AzureStorage;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;
using System.Threading.Tasks;
using Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain;

namespace Lykke.blue.Service.ReferralLinks.AzureRepositories.Offchain
{
    public class OffchainTransferEntity : BaseEntity, IOffchainTransfer
    {
        public string Id => RowKey;
        public string ClientId { get; set; }
        public string AssetId { get; set; }
        public decimal Amount { get; set; }
        public bool Completed { get; set; }
        public string OrderId { get; set; }
        public DateTime CreatedDt { get; set; }
        public string ExternalTransferId { get; set; }
        public OffchainTransferType Type { get; set; }
        public bool ChannelClosing { get; set; }
        public bool Onchain { get; set; }
        public bool IsChild { get; set; }
        public string ParentTransferId { get; set; }
        public string AdditionalDataJson { get; set; }
        public string BlockchainHash { get; set; }

        public class ByCommon
        {
            public static string GeneratePartitionKey()
            {
                return "OffchainTransfer";
            }

            public static OffchainTransferEntity Create(string id, string clientId, string assetId, decimal amount, OffchainTransferType type, string externalTransferId,
                string orderId = null, bool channelClosing = false, bool onchain = false)
            {
                return new OffchainTransferEntity
                {
                    PartitionKey = GeneratePartitionKey(),
                    RowKey = id,
                    AssetId = assetId,
                    Amount = amount,
                    ClientId = clientId,
                    OrderId = orderId,
                    CreatedDt = DateTime.UtcNow,
                    ExternalTransferId = externalTransferId,
                    Type = type,
                    ChannelClosing = channelClosing,
                    Onchain = onchain
                };
            }
        }
    }


    public class OffchainTransferRepository : IOffchainTransferRepository
    {
        private readonly INoSQLTableStorage<OffchainTransferEntity> _storage;

        public OffchainTransferRepository(INoSQLTableStorage<OffchainTransferEntity> storage)
        {
            _storage = storage;
        }

        public async Task<IOffchainTransfer> CreateTransfer(string transactionId, string clientId, string assetId, decimal amount, OffchainTransferType type, string externalTransferId, string orderId, bool channelClosing = false)
        {
            var entity = OffchainTransferEntity.ByCommon.Create(transactionId, clientId, assetId, amount, type, externalTransferId, orderId, channelClosing);

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

        public async Task<IEnumerable<IOffchainTransfer>> GetTransfersByDate(OffchainTransferType type, DateTimeOffset @from, DateTimeOffset to)
        {
            var filter1 = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal,
                    OffchainTransferEntity.ByCommon.GeneratePartitionKey()),
                TableOperators.And,
                TableQuery.GenerateFilterConditionForInt("Type", QueryComparisons.Equal, (int)type)
            );

            var filter2 = TableQuery.CombineFilters(
                TableQuery.GenerateFilterConditionForDate("CreatedDt", QueryComparisons.GreaterThanOrEqual, from),
                TableOperators.And,
                TableQuery.GenerateFilterConditionForDate("CreatedDt", QueryComparisons.LessThanOrEqual, to)
            );

            var query = new TableQuery<OffchainTransferEntity>().Where(
                TableQuery.CombineFilters(filter1, TableOperators.And, filter2));

            return await _storage.WhereAsync(query);
        }
    }
}
