namespace Lykke.blue.Service.ReferralLinks.Core.Domain.Client
{
    public class IsOffchainUserSettings : TraderSettingsBase
    {
        public override string GetId()
        {
            return "IsOffchainUserSettings";
        }

        public bool IsOffchain { get; } = true;

        public static IsOffchainUserSettings CreateDefault()
        {
            return new IsOffchainUserSettings();
        }
    }
}
