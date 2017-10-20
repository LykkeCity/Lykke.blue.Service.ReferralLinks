using Lykke.Service.ReferralLinks.Core.Domain.WalletCredentials;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.ReferralLinks.AzureRepositories.WalletCredentials
{
    public interface IWalletCredentialsRepository
    {
        Task<IWalletCredentials> GetAsync(string clientId);
    }
}
