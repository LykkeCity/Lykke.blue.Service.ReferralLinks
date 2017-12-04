using Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain;

namespace Lykke.blue.Service.ReferralLinks.Models.Offchain
{
    public class OffchainTradeRespModel
    {
        public string TransferId { get; set; }
        public string TransactionHex { get; set; }
        public OffchainOperationResult OperationResult { get; set; }
    }
}
