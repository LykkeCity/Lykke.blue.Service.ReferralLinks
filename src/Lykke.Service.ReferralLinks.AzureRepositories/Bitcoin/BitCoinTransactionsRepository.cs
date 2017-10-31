using AzureStorage;
using Lykke.Service.ReferralLinks.Core.BitCoinApi;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.ReferralLinks.AzureRepositories.Bitcoin
{
    public class BitCoinTransactionEntity : TableEntity, IBitcoinTransaction
    {
        public static class ByTransactionId
        {
            public static string GeneratePartitionKey()
            {
                return "TransId";
            }

            public static string GenerateRowKey(string transactionId)
            {
                return transactionId;
            }

            public static BitCoinTransactionEntity CreateNew(string transactionId, string commandType, string requestData, string contextData, string response, string blockchainHash = null)
            {
                var result = new BitCoinTransactionEntity
                {
                    PartitionKey = GeneratePartitionKey(),
                    RowKey = GenerateRowKey(transactionId),
                    CommandType = commandType,
                    Created = DateTime.UtcNow,
                    ResponseData = response,
                    BlockchainHash = blockchainHash
                };

                result.SetData(requestData, contextData);
                return result;
            }
        }

        public string TransactionId => RowKey;
        public DateTime Created { get; set; }
        public DateTime? ResponseDateTime { get; set; }
        public string CommandType { get; set; }
        public string RequestData { get; set; }
        public string ResponseData { get; set; }
        public string ContextData { get; set; }
        public string BlockchainHash { get; set; }

        internal void SetData(string requestData, string contextData)
        {
            RequestData = requestData;
            ContextData = contextData;
        }


        internal void UpdateResponse(string resp, DateTime? dateTime)
        {
            ResponseData = resp;
            ResponseDateTime = dateTime ?? DateTime.UtcNow;
        }
    }

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
