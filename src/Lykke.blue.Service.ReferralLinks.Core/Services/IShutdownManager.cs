using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.Core.Services
{
    public interface IShutdownManager
    {
        Task StopAsync();
    }
}