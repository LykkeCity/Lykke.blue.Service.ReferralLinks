namespace Lykke.blue.Service.ReferralLinks.Models.Offchain
{
    public class ApiOffchainOrder
    {
        public string Id { get; set; }

        public string DateTime { get; set; }

        public string OrderType { get; set; }

        public double Volume { get; set; }

        public double Price { get; set; }

        public string Asset { get; set; }

        public string AssetPair { get; set; }

        public double TotalCost { get; set; }

    }
}
