using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain
{
    public interface IOffchainTransferRepository
    {
        Task<IOffchainTransfer> CreateTransfer(string transactionId, string clientId, string assetId, decimal amount, OffchainTransferType type, string externalTransferId, string orderId, bool channelClosing = false);
        Task<IOffchainTransfer> GetTransfer(string id);
        Task CompleteTransfer(string transferId, bool? onchain = null, string blockchainHash = null);
        Task UpdateTransfer(string transferId, string toString, bool closing = false, bool? onchain = null);
    }
}
