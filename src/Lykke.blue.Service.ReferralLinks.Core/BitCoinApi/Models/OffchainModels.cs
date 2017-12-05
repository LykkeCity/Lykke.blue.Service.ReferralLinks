using System;

namespace Core.BitCoin.BitcoinApi.Models
{
    public class OffchainTransferData
    {
        public string ClientPubKey { get; set; }
        public decimal Amount { get; set; }
        public string AssetId { get; set; }
        public string ClientPrevPrivateKey { get; set; }
        public string ExternalTransferId { get; set; }
        public bool Required { get; set; }
    }

    public class CreateChannelData
    {
        public string ClientPubKey { get; set; }
        public decimal ClientAmount { get; set; }
        public decimal HubAmount { get; set; }
        public string AssetId { get; set; }
        public string ExternalTransferId { get; set; }
        public bool Required { get; set; }
    }

    public class CreateHubComitmentData
    {
        public string ClientPubKey { get; set; }
        public decimal Amount { get; set; }
        public string AssetId { get; set; }
        public string SignedByClientChannel { get; set; }
    }

    public class FinalizeData
    {
        public string ClientPubKey { get; set; }
        public string AssetId { get; set; }
        public string ClientRevokePubKey { get; set; }
        public string SignedByClientHubCommitment { get; set; }
        public string ExternalTransferId { get; set; }
        public string OffchainTransferId { get; set; }
    }

   

    public class CloseChannelData
    {
        public string ClientPubKey { get; set; }
        public string AssetId { get; set; }
        public string SignedClosingTransaction { get; set; }
        public string OffchainTransferId { get; set; }
    }

   

    public class OffchainBaseResponse
    {
        public ErrorResponse Error { get; set; }

        public bool HasError => Error != null;

        public string TxHash { get; set; }
    }

    public class OffchainResponse : OffchainBaseResponse
    {
        public string Transaction { get; set; }
        public Guid? TransferId { get; set; }
    }

    public class OffchainClosingResponse : OffchainResponse
    {
        public bool ChannelClosing { get; set; }
    }

   

   

    public class OffchainChannelBalance
    {
        public string Multisig { get; set; }
        public decimal ClientAmount { get; set; }
        public decimal HubAmount { get; set; }
        public string Hash { get; set; }
        public bool Actual { get; set; }
    }
}
