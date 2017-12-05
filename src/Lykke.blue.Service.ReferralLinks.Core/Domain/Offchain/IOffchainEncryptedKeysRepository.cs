using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain
{
    public interface IOffchainEncryptedKeysRepository
    {
        Task<IOffchainEncryptedKey> GetKey(string clientId, string asset);

        Task UpdateKey(string clientId, string asset, string key);
    }
}
