using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain
{
    public interface IOffchainOrdersRepository
    {
        Task<IOffchainOrder> GetOrder(string id);
        Task UpdatePrice(string orderId, decimal price);
    }
}
