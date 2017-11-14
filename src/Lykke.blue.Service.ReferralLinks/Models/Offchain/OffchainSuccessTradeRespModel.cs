using Lykke.Blue.Service.ReferralLinks.Core.Domain.Offchain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Blue.Service.ReferralLinks.Models.Offchain
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
