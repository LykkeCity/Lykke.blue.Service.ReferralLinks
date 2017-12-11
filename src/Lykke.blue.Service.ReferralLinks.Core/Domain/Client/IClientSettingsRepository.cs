using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.Core.Domain.Client
{
    public interface IClientSettingsRepository
    {
        Task<T> GetSettings<T>(string traderId) where T : TraderSettingsBase, new();
    }
}
