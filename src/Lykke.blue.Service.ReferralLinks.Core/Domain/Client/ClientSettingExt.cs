using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.Core.Domain.Client
{
    public static class ClientSettingExt
    {
        public static async Task<bool> IsOffchainClient(this IClientSettingsRepository repository, string clientId)
        {
            var setting = await repository.GetSettings<IsOffchainUserSettings>(clientId);

            return setting.IsOffchain;
        }        
    }
}