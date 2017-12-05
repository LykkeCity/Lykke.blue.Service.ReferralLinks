using Lykke.blue.Service.ReferralLinks.Core.Settings.ServiceSettings;

namespace Lykke.blue.Service.ReferralLinks.Core.Settings
{
    public class AppSettings
    {
        public ReferralLinksSettings ReferralLinksService { get; set; }       
        public MatchingEngineClient MatchingEngineClient { get; set; }        
    }
}
