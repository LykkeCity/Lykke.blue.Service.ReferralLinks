using Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain;
using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.Services.Offchain
{
    public class OffchainRequestService : IOffchainRequestService
    {
        private readonly IOffchainRequestRepository _offchainRequestRepository;
        private readonly IOffchainTransferRepository _offchainTransferRepository;

        public OffchainRequestService(IOffchainRequestRepository offchainRequestRepository, IOffchainTransferRepository offchainTransferRepository/*, IClientSettingsRepository clientSettingsRepository, IClientAccountsRepository clientAccountsRepository, IAppNotifications appNotifications*/)
        {
            _offchainRequestRepository = offchainRequestRepository;
            _offchainTransferRepository = offchainTransferRepository;
        }

        public async Task CreateOffchainRequest(string transactionId, string clientId, string assetId, decimal amount, string orderId, OffchainTransferType type)
        {
            var transfer = await _offchainTransferRepository.CreateTransfer(transactionId, clientId, assetId, amount, type, null, orderId);

            await _offchainRequestRepository.CreateRequest(transfer.Id, clientId, assetId, RequestType.RequestTransfer, type);
        }        
    }
}
