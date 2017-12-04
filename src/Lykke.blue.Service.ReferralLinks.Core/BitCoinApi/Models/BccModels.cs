namespace Core.BitCoin.BitcoinApi.Models
{
    public class BitcoinBaseResponse
    {
        public ErrorResponse Error { get; set; }

        public bool HasError => Error != null;
    }

    public class BccSplitTransactionResponse : BitcoinBaseResponse
    {
        public string Transaction { get; set; }

        public decimal ClientAmount { get; set; }

        public decimal HubAmount { get; set; }

        public decimal ClientFeeAmount { get; set; }

        public string Outputs { get; set; }
    }

    public class BccTransactionHashResponse : BitcoinBaseResponse
    {
        public string TransactionHash { get; set; }
    }

    public class BccTransactionResponse : BitcoinBaseResponse
    {
        public string Transaction { get; set; }
    }
}
