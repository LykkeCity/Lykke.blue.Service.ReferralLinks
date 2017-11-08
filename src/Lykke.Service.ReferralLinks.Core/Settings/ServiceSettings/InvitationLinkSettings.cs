using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.ReferralLinks.Core.Settings.ServiceSettings
{
    public class InvitationLinkSettings
    {
        public int MaxNumOfClientsToReceiveReward { get; set; }
        public decimal RewardAmount { get; set; }
        public string RewardAsset { get; set; }
        public int LinksNumberLimitPerSender { get; set; }
    }
}
