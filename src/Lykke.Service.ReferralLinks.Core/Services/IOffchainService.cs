using Lykke.Service.ReferralLinks.Core.Domain.Offchain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.ReferralLinks.Core.Services
{
    public interface IOffchainService
    {
        Task<OffchainResult> CreateDirectTransfer(string clientId, string asset, decimal amount, string prevTempPrivateKey);
        Task<OffchainResult> CreateHubCommitment(string clientId, string transferId, string signedChannel);
        Task<OffchainResult> Finalize(string clientId, string transferId, string clientRevokePubKey, string clientRevokeEncryptedPrivateKey, string signedCommitment);
        Task<OffchainResultOrder> GetResultOrderFromTransfer(string transferId);
    }
}
