using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain
{
    public interface IOffchainRequestRepository
    {
        Task<IOffchainRequest> CreateRequest(string transferId, string clientId, string assetId, RequestType type, OffchainTransferType transferType);

        Task<IEnumerable<IOffchainRequest>> GetRequestsForClient(string clientId);

        Task Complete(string requestId);
    }
}
