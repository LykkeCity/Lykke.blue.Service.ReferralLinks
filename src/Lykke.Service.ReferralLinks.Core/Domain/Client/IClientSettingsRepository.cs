using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.ReferralLinks.Core.Domain.Client
{
    public abstract class TraderSettingsBase
    {
        public abstract string GetId();


        public static T CreateDefault<T>() where T : TraderSettingsBase, new()
        {
            if (typeof(T) == typeof(IsOffchainUserSettings))
                return IsOffchainUserSettings.CreateDefault() as T;

            return new T();
        }
    }

    public class IsOffchainUserSettings : TraderSettingsBase
    {
        public override string GetId()
        {
            return "IsOffchainUserSettings";
        }

        public bool IsOffchain { get; set; } = true;

        public static IsOffchainUserSettings CreateDefault()
        {
            return new IsOffchainUserSettings();
        }
    }

    public interface IClientSettingsRepository
    {
        Task<T> GetSettings<T>(string traderId) where T : TraderSettingsBase, new();
    }

    public static class ClientSettingExt
    {
        public static async Task<bool> IsOffchainClient(this IClientSettingsRepository repository, string clientId)
        {
            var setting = await repository.GetSettings<IsOffchainUserSettings>(clientId);

            return setting.IsOffchain;
        }        
    }
}
