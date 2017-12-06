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
        string ExternalTransferId { get; }
        OffchainTransferType Type { get; }
        bool ChannelClosing { get; }
    }
}
