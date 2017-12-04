using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.Core.Services
{
    public interface IStatisticsService
    {
        Task<IReferralLinksStatistics> GetStatistics(string senderClientId);
    }
}
