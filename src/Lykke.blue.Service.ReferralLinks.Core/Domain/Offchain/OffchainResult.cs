﻿namespace Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain
{
    public class OffchainResult
    {
        public string TransferId { get; set; }
        public string TransactionHex { get; set; }
        public OffchainOperationResult OperationResult { get; set; }
    }
}
