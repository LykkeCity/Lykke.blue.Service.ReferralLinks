using Lykke.Service.ReferralLinks.Core.Settings.SlackNotifications;

namespace Lykke.Service.ReferralLinks.Core.Settings.ServiceSettings
{
    public class ReferralLinksSettings
    {
        public DbSettings Db { get; set; }
        public InvitationLinkSettings InvitationLinkSettings { get; set; }
        public GiftCoinsLinkSettings GiftCoinsLinkSettings { get; set; }
        public Services ExternalServices { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
        public FirebaseSettings Firebase { get; set; }        
        public string LykkeReferralClientId { get; set; }
       
    }
}
