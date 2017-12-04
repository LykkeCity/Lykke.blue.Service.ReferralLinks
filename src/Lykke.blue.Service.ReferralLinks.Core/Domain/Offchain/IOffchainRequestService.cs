using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain
{
    public interface IOffchainRequestService
    {
        Task CreateOffchainRequest(string transactionId, string clientId, string assetId, decimal amount, string orderId, OffchainTransferType type);
    }
}
