using Lykke.blue.Service.ReferralLinks.Core.Domain.WalletCredentials;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.blue.Service.ReferralLinks.AzureRepositories.WalletCredentials
{
    public class WalletCredentialsEntity : TableEntity, IWalletCredentials
    {
        public static class ByClientId
        {
            public static string GeneratePartitionKey()
            {
                return "Wallet";
            }

            public static string GenerateRowKey(string clientId)
            {
                return clientId;
            }

        }

        public string ClientId { get; set; }
        public string PublicKey { get; set; }
        public string MultiSig { get; set; }
    }
}
