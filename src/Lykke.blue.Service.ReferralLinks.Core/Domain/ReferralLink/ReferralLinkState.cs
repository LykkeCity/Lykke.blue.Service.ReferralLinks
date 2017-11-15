namespace Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink
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
