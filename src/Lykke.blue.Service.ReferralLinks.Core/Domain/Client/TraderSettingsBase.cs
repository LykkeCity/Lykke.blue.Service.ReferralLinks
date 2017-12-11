namespace Lykke.blue.Service.ReferralLinks.Core.Domain.Client
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
}
