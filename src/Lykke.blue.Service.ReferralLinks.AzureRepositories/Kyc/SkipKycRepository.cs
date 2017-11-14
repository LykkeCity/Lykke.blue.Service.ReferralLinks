using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AzureStorage;

using Microsoft.WindowsAzure.Storage.Table;
using Lykke.Blue.Service.ReferralLinks.Core.Kyc;

namespace Lykke.Blue.Service.ReferralLinks.AzureRepositories.Kyc
{
    public class SkipKycClientEntity : TableEntity
    {
        public static string GeneratePartition()
        {
            return "SkipKyc";
        }

        public static string GenerateRowKey(string clientId)
        {
            return clientId;
        }

        public static SkipKycClientEntity Create(string clientId)
        {
            return new SkipKycClientEntity
            {
                PartitionKey = GeneratePartition(),
                RowKey = GenerateRowKey(clientId)
            };
        }

        public string ClientId => RowKey;
    }

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
