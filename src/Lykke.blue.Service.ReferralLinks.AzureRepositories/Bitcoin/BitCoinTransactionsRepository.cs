using AzureStorage;
using Lykke.blue.Service.ReferralLinks.Core.BitCoinApi;
using System;
using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.AzureRepositories.Bitcoin
{
    public class BitCoinTransactionsRepository : IBitCoinTransactionsRepository
    {
        private readonly INoSQLTableStorage<BitCoinTransactionEntity> _tableStorage;

        public BitCoinTransactionsRepository(INoSQLTableStorage<BitCoinTransactionEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task CreateAsync(string transactionId, string commandType,
            string requestData, string contextData, string response, string blockchainHash = null)
        {
            var newEntity = BitCoinTransactionEntity.ByTransactionId.CreateNew(transactionId, commandType, requestData, contextData, response, blockchainHash);
            await _tableStorage.InsertAsync(newEntity);
        }

        public async Task<IBitcoinTransaction> FindByTransactionIdAsync(string transactionId)
        {
            var partitionKey = BitCoinTransactionEntity.ByTransactionId.GeneratePartitionKey();
            var rowKey = BitCoinTransactionEntity.ByTransactionId.GenerateRowKey(transactionId);
            return await _tableStorage.GetDataAsync(partitionKey, rowKey);
        }

        public async Task<IBitcoinTransaction> SaveResponseAndHashAsync(string transactionId, string resp, string hash, DateTime? dateTime = null)
        {
            var partitionKey = BitCoinTransactionEntity.ByTransactionId.GeneratePartitionKey();
            var rowKey = BitCoinTransactionEntity.ByTransactionId.GenerateRowKey(transactionId);

            return await _tableStorage.MergeAsync(partitionKey, rowKey, entity =>
            {
                entity.UpdateResponse(resp, dateTime);
                entity.BlockchainHash = hash;
                return entity;
            });
        }

        public async Task UpdateAsync(string transactionId, string requestData, string contextData, string response)
        {
            var partitionKey = BitCoinTransactionEntity.ByTransactionId.GeneratePartitionKey();
            var rowKey = BitCoinTransactionEntity.ByTransactionId.GenerateRowKey(transactionId);

            await _tableStorage.MergeAsync(partitionKey, rowKey, entity =>
            {
                entity.RequestData = requestData ?? entity.RequestData;
                entity.ContextData = contextData ?? entity.ContextData;
                entity.ResponseData = response ?? entity.ResponseData;
                return entity;
            });
        }

        public Task DeleteAsync(string transactionId)
        {
            var partitionKey = BitCoinTransactionEntity.ByTransactionId.GeneratePartitionKey();
            var rowKey = BitCoinTransactionEntity.ByTransactionId.GenerateRowKey(transactionId);

            return _tableStorage.DeleteAsync(partitionKey, rowKey);
        }
    }
}
