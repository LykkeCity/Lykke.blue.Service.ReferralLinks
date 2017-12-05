using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain
{
    public interface IOffchainOrdersRepository
    {
        Task<IOffchainOrder> GetOrder(string id);
        Task<IOffchainOrder> CreateOrder(string clientId, string asset, string assetPair, decimal volume, decimal reservedVolume, bool straight);
        Task<IOffchainOrder> CreateLimitOrder(string clientId, string asset, string assetPair, decimal volume, decimal reservedVolume, bool straight, decimal price);
        Task UpdatePrice(string orderId, decimal price);
    }
}
