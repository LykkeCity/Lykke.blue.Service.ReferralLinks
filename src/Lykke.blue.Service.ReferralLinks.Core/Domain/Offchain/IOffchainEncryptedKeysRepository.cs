using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain
{
    public interface IOffchainEncryptedKey
    {
        string ClientId { get; }
        string Asset { get; }
        string Key { get; }
    }

    public interface IOffchainEncryptedKeysRepository
    {
        Task<IOffchainEncryptedKey> GetKey(string clientId, string asset);

        Task UpdateKey(string clientId, string asset, string key);
    }
}
