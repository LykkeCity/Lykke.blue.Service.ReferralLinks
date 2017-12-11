using System;

namespace Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain
{
    public interface IOffchainRequest
    {
        string RequestId { get; }
        string TransferId { get; }

        string AssetId { get; }
        string ClientId { get; }

        RequestType Type { get; }

        DateTime? StartProcessing { get; }

        DateTime CreateDt { get; }

        int TryCount { get; }

        OffchainTransferType TransferType { get; }

        DateTime? ServerLock { get; }
    }
}