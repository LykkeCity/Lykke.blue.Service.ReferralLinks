using Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain;

namespace Lykke.blue.Service.ReferralLinks.Models.Offchain
{
    public class OffchainSuccessTradeRespModel
    {
        public string TransferId { get; set; }
        public string TransactionHex { get; set; }
        public OffchainOperationResult OperationResult { get; set; }

        public ApiOffchainOrder Order { get; set; }
    }

    public class ApiOffchainOrder
    {
        public string Id { get; set; }

        public string DateTime { get; set; }

        public string OrderType { get; set; }

        public double Volume { get; set; }

        public double Price { get; set; }

        public string Asset { get; set; }

        public string AssetPair { get; set; }

        public double TotalCost { get; set; }

        public double RemainingVolume { get; set; }

        public double RemainingOtherVolume { get; set; }
    }
}
