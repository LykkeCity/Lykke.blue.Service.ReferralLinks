namespace Lykke.blue.Service.ReferralLinks.Core.Domain.WalletCredentials
{
    public interface IWalletCredentials
    {
        string ClientId { get; }
        string PublicKey { get; }
    }
}
