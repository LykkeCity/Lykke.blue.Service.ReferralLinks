using System.Threading.Tasks;

namespace Lykke.Blue.Service.ReferralLinks.Core.Services
{
    public interface IShutdownManager
    {
        Task StopAsync();
    }
}