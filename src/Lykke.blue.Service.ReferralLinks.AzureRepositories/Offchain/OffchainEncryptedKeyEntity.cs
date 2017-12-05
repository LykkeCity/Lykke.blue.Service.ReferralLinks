using Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain;

namespace Lykke.blue.Service.ReferralLinks.AzureRepositories.Offchain
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
}