using AzureStorage;
using Lykke.Service.ReferralLinks.Core.Domain.Offchain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.ReferralLinks.AzureRepositories.Offchain
{
    public class OffchainEncryptedKeyEntity : BaseEntity, IOffchainEncryptedKey
    {
        public string ClientId => RowKey;
        public string Asset => PartitionKey;
        public string Key { get; set; }

        public static OffchainEncryptedKeyEntity Create(string clientId, string asset, string key)
        {
            return new OffchainEncryptedKeyEntity
            {
                PartitionKey = asset,
                RowKey = clientId,
                Key = key
            };
        }
    }

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
