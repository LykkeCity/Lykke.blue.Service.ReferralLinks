using AzureStorage;
using Lykke.blue.Service.ReferralLinks.Core.Kyc;
using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.AzureRepositories.Kyc
{
    public class SkipKycRepository : ISkipKycRepository
    {
        private readonly INoSQLTableStorage<SkipKycClientEntity> _tableStorage;

        public SkipKycRepository(INoSQLTableStorage<SkipKycClientEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<bool> CanSkipKyc(string clientId)
        {
            var entity = await _tableStorage.GetDataAsync(SkipKycClientEntity.GeneratePartition(), SkipKycClientEntity.GenerateRowKey(clientId));
            return entity != null;
        }

        public async Task SkipKyc(string clientId, bool skip)
        {
            if (skip)
                await _tableStorage.InsertOrReplaceAsync(SkipKycClientEntity.Create(clientId));
            else
                await _tableStorage.DeleteIfExistAsync(SkipKycClientEntity.GeneratePartition(), SkipKycClientEntity.GenerateRowKey(clientId));
        }
    }
}
