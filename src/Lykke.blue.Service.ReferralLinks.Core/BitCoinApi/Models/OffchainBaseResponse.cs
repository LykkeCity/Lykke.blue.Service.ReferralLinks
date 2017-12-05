namespace Lykke.blue.Service.ReferralLinks.Core.BitCoinApi.Models
{
    public class OffchainBaseResponse
    {
        public ErrorResponse Error { get; set; }

        public bool HasError => Error != null;

        public string TxHash { get; set; }
    }
}
