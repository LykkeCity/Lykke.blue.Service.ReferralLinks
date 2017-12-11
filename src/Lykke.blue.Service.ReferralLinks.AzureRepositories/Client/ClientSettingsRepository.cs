using AzureStorage;
using Lykke.blue.Service.ReferralLinks.Core.Domain.Client;
using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.AzureRepositories.Client
{
    public class ClientSettingsRepository : IClientSettingsRepository
    {
        private readonly INoSQLTableStorage<ClientSettingsEntity> _tableStorage;

        public ClientSettingsRepository(INoSQLTableStorage<ClientSettingsEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<T> GetSettings<T>(string traderId) where T : TraderSettingsBase, new()
        {
            var partitionKey = ClientSettingsEntity.GeneratePartitionKey(traderId);
            var defaultValue = TraderSettingsBase.CreateDefault<T>();
            var rowKey = ClientSettingsEntity.GenerateRowKey(defaultValue);
            var entity = await _tableStorage.GetDataAsync(partitionKey, rowKey);
            return entity == null ? defaultValue : entity.GetSettings<T>();
        }       
    }
}
