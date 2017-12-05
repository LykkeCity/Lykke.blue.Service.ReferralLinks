using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.Core.Domain.WalletCredentials
{
    public interface IWalletCredentialsRepository
    {
        Task<IWalletCredentials> GetAsync(string clientId);
    }
}
