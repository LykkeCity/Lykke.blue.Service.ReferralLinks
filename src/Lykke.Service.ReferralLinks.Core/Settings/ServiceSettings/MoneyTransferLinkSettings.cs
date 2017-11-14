using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Blue.Service.ReferralLinks.Core.Settings.ServiceSettings
{
    public class GiftCoinsLinkSettings
    {
        public int ExpirationDaysLimit { get; set; }
        public int ExpiredLinksCheckTimeoutMinutes { get; set; }
    }
}
