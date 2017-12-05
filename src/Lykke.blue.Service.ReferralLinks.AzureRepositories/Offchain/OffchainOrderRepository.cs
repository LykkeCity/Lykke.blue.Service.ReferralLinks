using AzureStorage;
using Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain;
using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.AzureRepositories.Offchain
{
    public class OffchainOrderRepository : IOffchainOrdersRepository
    {
        private readonly INoSQLTableStorage<OffchainOrder> _storage;

        public OffchainOrderRepository(INoSQLTableStorage<OffchainOrder> storage)
        {
            _storage = storage;
        }

        public async Task<IOffchainOrder> GetOrder(string id)
        {
            return await _storage.GetDataAsync(OffchainOrder.GeneratePartitionKey(), id);
        }

        public async Task<IOffchainOrder> CreateOrder(string clientId, string asset, string assetPair, decimal volume, decimal reservedVolume, bool straight)
        {
            var entity = OffchainOrder.Create(clientId, asset, assetPair, volume, reservedVolume, straight);
            await _storage.InsertAsync(entity);
            return entity;
        }

        public async Task<IOffchainOrder> CreateLimitOrder(string clientId, string asset, string assetPair, decimal volume, decimal reservedVolume, bool straight, decimal price)
        {
            var entity = OffchainOrder.Create(clientId, asset, assetPair, volume, reservedVolume, straight, price);
            await _storage.InsertAsync(entity);
            return entity;
        }

        public Task UpdatePrice(string orderId, decimal price)
        {
            return _storage.ReplaceAsync(OffchainOrder.GeneratePartitionKey(), orderId, order =>
            {
                order.Price = price;
                return order;
            });
        }
    }
}
