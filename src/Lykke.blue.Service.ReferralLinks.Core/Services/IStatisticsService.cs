using System.Threading.Tasks;
using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;

namespace Lykke.blue.Service.ReferralLinks.Core.Services
{
    public interface IStatisticsService
    {
        Task<IReferralLinksStatistics> GetStatistics(string senderClientId);
    }
}
