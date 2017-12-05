using System.Collections.Generic;

namespace Lykke.blue.Service.ReferralLinks.Core.BitCoinApi
{
    public class SwapOffchainContextData : BaseContextData
    {
        public class Operation
        {
            public string ClientId { get; set; }
            public decimal Amount { get; set; }
            public string AssetId { get; set; }
            public string TransactionId { get; set; }
            public string ClientTradeId { get; set; }
        }

        public List<Operation> Operations { get; set; } = new List<Operation>();
    }
}