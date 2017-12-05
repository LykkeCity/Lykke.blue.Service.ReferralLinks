using AzureStorage;
using Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain;
using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.AzureRepositories.Offchain
{
    public class OffchainEncryptedKeyRepository : IOffchainEncryptedKeysRepository
    {
        private readonly INoSQLTableStorage<OffchainEncryptedKeyEntity> _storage;

        public OffchainEncryptedKeyRepository(INoSQLTableStorage<OffchainEncryptedKeyEntity> storage)
        {
            _storage = storage;
        }

        public async Task<IOffchainEncryptedKey> GetKey(string clientId, string asset)
        {
            return await _storage.GetDataAsync(asset, clientId);
        }

        public Task UpdateKey(string clientId, string asset, string key)
        {
            return _storage.InsertOrReplaceAsync(OffchainEncryptedKeyEntity.Create(clientId, asset, key));
        }
    }
}
