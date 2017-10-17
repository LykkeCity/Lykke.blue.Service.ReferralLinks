using System.Threading.Tasks;

namespace Lykke.Service.ReferralLinks.Core.Services
{
    public interface IShutdownManager
    {
        Task StopAsync();
    }
}