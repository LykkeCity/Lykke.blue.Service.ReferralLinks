using System;
using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.Core.BitCoinApi
{

    public interface IBitcoinTransaction
    {
        string TransactionId { get; }
        DateTime Created { get; }
        DateTime? ResponseDateTime { get; }
        string CommandType { get; }
        string RequestData { get; }
        string ResponseData { get; }
        string ContextData { get; }
        string BlockchainHash { get; }
    }

    public interface IBitCoinTransactionsRepository
    {
        Task CreateAsync(string transactionId, string commandType, string requestData, string contextData, string response, string blockchainHash = null);
        Task<IBitcoinTransaction> FindByTransactionIdAsync(string transactionId);
        Task<IBitcoinTransaction> SaveResponseAndHashAsync(string transactionId, string resp, string hash, DateTime? dateTime = null);
        Task UpdateAsync(string transactionId, string requestData, string contextData, string response);
        Task DeleteAsync(string transactionId);
    }

   
}
