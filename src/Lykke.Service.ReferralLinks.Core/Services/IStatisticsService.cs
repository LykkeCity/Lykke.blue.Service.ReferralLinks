using Lykke.Blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Blue.Service.ReferralLinks.Core.Services
{
    public interface IStatisticsService
    {
        Task<IReferralLinksStatistics> GetStatistics(string senderClientId);
    }
}
