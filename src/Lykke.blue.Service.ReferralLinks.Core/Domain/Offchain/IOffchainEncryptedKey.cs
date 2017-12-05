namespace Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain
{
    public interface IOffchainEncryptedKey
    {
        string ClientId { get; }
        string Asset { get; }
        string Key { get; }
    }
}