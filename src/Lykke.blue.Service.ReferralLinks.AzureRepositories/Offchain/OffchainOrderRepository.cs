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
