using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Blue.Service.ReferralLinks.Core.BitCoinApi
{
    public class BaseContextData
    {
        public string[] SignsClientIds { get; set; }
    }

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
