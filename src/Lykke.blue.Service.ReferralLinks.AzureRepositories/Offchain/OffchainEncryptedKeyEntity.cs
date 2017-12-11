using Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain;

namespace Lykke.blue.Service.ReferralLinks.AzureRepositories.Offchain
{
    public class OffchainEncryptedKeyEntity : BaseEntity, IOffchainEncryptedKey
    {
        public string Key { get; private set; }

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
