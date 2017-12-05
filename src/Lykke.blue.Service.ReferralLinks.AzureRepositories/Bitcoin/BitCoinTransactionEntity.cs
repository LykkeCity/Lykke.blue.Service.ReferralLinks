using System;
using Lykke.blue.Service.ReferralLinks.Core.BitCoinApi;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.blue.Service.ReferralLinks.AzureRepositories.Bitcoin
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
}