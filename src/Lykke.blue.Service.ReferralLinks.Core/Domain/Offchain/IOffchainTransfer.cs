namespace Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain
{
    //OffchainService and all offchain functionality will be removed in next PR. No need to review.
    public interface IOffchainTransfer
    {
        string Id { get; }
        string ClientId { get; }
        string AssetId { get; }
        decimal Amount { get; }
        bool Completed { get; }
        string ExternalTransferId { get; }
        OffchainTransferType Type { get; }
        bool ChannelClosing { get; }
    }
}
