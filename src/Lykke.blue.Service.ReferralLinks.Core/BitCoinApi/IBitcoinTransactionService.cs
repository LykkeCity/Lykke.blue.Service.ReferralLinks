using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.Core.BitCoinApi
{
    public interface IBitcoinTransactionService
    {
        Task SetTransactionContext<T>(string transactionId, T context) where T : BaseContextData;
    }
}
