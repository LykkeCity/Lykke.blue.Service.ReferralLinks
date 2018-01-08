using System.Collections.Generic;
using System.Threading.Tasks;

//OffchainService and all offchain functionality will be removed in next PR. No need to review.

namespace Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain
{
    public interface IOffchainRequestRepository
    {
        Task<IOffchainRequest> CreateRequest(string transferId, string clientId, string assetId, RequestType type, OffchainTransferType transferType);

        Task<IEnumerable<IOffchainRequest>> GetRequestsForClient(string clientId);

        Task Complete(string requestId);
    }
}
