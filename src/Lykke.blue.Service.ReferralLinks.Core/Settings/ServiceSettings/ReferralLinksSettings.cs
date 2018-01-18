// ReSharper disable ClassNeverInstantiated.Global
namespace Lykke.blue.Service.ReferralLinks.Core.Settings.ServiceSettings
{
    public class ReferralLinksSettings
    {
        public DbSettings Db { get; set; }
        public InvitationLinkSettings InvitationLinkSettings { get; set; }
        public GiftCoinsLinkSettings GiftCoinsLinkSettings { get; set; }
        public FirebaseSettings Firebase { get; set; }        
        public string LykkeReferralClientId { get; set; }
    }
}
