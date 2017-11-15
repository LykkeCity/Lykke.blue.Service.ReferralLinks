using Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.blue.Service.ReferralLinks.Models.Offchain
{
    public class OffchainTradeRespModel
    {
        public string TransferId { get; set; }
        public string TransactionHex { get; set; }
        public OffchainOperationResult OperationResult { get; set; }
    }
}
