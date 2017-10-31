using Lykke.Service.ReferralLinks.Core.Settings.SlackNotifications;

namespace Lykke.Service.ReferralLinks.Core.Settings.ServiceSettings
{
    public class ReferralLinksSettings
    {
        public DbSettings Db { get; set; }
        public int ExpirationDaysLimit { get; set; }
        public int ExpiredLinksCheckTimeout { get; set; }
        public Services ExternalServices { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
    }
}
