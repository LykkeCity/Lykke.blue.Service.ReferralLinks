using AzureStorage;
using Lykke.blue.Service.ReferralLinks.Core.BitCoinApi;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.AzureRepositories.Bitcoin
{
    public class BitcoinTransactionContextBlobStorage : IBitcoinTransactionContextBlobStorage
    {
        private const string BlobContainer = "bitcoin-transaction-context";

        private readonly IBlobStorage _storage;

        public BitcoinTransactionContextBlobStorage(IBlobStorage storage)
        {
            _storage = storage;
        }

        public async Task<string> Get(string transactionId)
        {
            if (await _storage.HasBlobAsync(BlobContainer, GetKey(transactionId)))
                return await _storage.GetAsTextAsync(BlobContainer, GetKey(transactionId));
            return null;
        }

        public async Task Set(string transactionId, string context)
        {
            await _storage.SaveBlobAsync(BlobContainer, GetKey(transactionId), Encoding.UTF8.GetBytes(context));
        }

        private string GetKey(string transactionId)
        {
            return $"{transactionId}.txt";
        }
    }
}
