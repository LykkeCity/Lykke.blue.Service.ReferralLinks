using System.Threading.Tasks;
using Lykke.blue.Service.ReferralLinks.Core.Domain.WalletCredentials;

namespace Lykke.blue.Service.ReferralLinks.AzureRepositories.WalletCredentials
{
    public interface IWalletCredentialsRepository
    {
        Task<IWalletCredentials> GetAsync(string clientId);
    }
}
