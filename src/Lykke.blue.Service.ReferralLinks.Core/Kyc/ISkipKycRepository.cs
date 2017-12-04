using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.Core.Kyc
{
    public interface ISkipKycRepository
    {
        Task<bool> CanSkipKyc(string clientId);
        Task SkipKyc(string clientId, bool skip);
    }
}
