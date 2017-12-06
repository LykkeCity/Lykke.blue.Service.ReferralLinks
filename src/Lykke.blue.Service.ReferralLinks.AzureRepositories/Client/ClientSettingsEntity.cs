using Lykke.blue.Service.ReferralLinks.Core.Domain.Client;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.blue.Service.ReferralLinks.AzureRepositories.Client
{
    public class ClientSettingsEntity : TableEntity
    {
        public static string GeneratePartitionKey(string traderId)
        {
            return traderId;
        }

        public static string GenerateRowKey(TraderSettingsBase settingsBase)
        {
            return settingsBase.GetId();
        }

        private string Data { get; set; }

        internal T GetSettings<T>() where T : TraderSettingsBase
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(Data);
        }

        private void SetSettings(TraderSettingsBase settings)
        {
            Data = Newtonsoft.Json.JsonConvert.SerializeObject(settings);
        }
    }
}
