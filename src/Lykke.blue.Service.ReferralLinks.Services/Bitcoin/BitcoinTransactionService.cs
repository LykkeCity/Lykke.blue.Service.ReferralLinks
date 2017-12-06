using Lykke.blue.Service.ReferralLinks.Core.BitCoinApi;
using Lykke.blue.Service.ReferralLinks.Core.Extensions;
using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.Services.Bitcoin
{
    public class BitcoinTransactionService : IBitcoinTransactionService
    {
        private readonly IBitcoinTransactionContextBlobStorage _contextBlobStorage;

        public BitcoinTransactionService(IBitcoinTransactionContextBlobStorage contextBlobStorage)
        {
            _contextBlobStorage = contextBlobStorage;
        }

       

        public Task SetTransactionContext<T>(string transactionId, T context) where T : BaseContextData
        {
            return _contextBlobStorage.Set(transactionId, context.ToJson());
        }

       
    }
}
