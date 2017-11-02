namespace Lykke.Service.ReferralLinks.Core.Domain.ReferralLink
{
    public enum ReferralLinkState
    {
        Created,
        SentToLykkeSharedWallet,
        Claimed,
        Expired,
        CoinsReturnedToSender
    }
}
