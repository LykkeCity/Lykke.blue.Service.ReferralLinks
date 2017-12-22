namespace Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink
{
    public interface IReferralLinkClaim
    {
        string Id { get; }
        string ReferralLinkId { get; }
        string RecipientClientId { get; }
        string RecipientTransactionId { get; set; }
        bool ShouldReceiveReward { get; }
        bool IsNewClient { get; }
        string ETag { get; set; }
    }
}
