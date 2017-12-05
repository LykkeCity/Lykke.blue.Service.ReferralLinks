using System;

namespace Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain
{
    public interface IOffchainTransfer
    {
        string Id { get; }
        string ClientId { get; }
        string AssetId { get; }
        decimal Amount { get; }
        bool Completed { get; }
        string OrderId { get; }
        DateTime CreatedDt { get; }
        string ExternalTransferId { get; }
        OffchainTransferType Type { get; }
        bool ChannelClosing { get; }
        bool Onchain { get; }
        bool IsChild { get; }
        string ParentTransferId { get; }
        string AdditionalDataJson { get; set; }
        string BlockchainHash { get; set; }
    }
}