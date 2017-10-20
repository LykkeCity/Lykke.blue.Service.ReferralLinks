using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.ReferralLinks.Core.Domain.Offchain
{
    public enum OffchainTransferType
    {
        None = 0,
        /// <summary>
        /// Money tranfsers from the client to the hub's reserve
        /// </summary>
        FromClient = 1,
        /// <summary>
        /// Money transfers from the hub's balance to the client
        /// </summary>
        FromHub = 2,
        /// <summary>
        /// User enters crypto money into their own wallet from an external source
        /// </summary>
        CashinFromClient = 3,
        /// <summary>
        /// User withdraws crypto money from their own wallet to an external source
        /// </summary>
        ClientCashout = 4,
        /// <summary>
        /// Not used
        /// </summary>
        FullCashout = 5,
        /// <summary>
        /// User enters money from the external account via SWIFT. Money will be transfered to the hub, 
        /// and colored coins will be transfered to the user's wallet from the hub
        /// </summary>
        CashinToClient = 6,
        /// <summary>
        /// User withdraws fiat money (ex: USD) from their wallet. Money will be transfered to the hub,
        /// and then will be transfered to the user's external account via SWIFT.
        /// </summary>
        OffchainCashout = 7,
        /// <summary>
        /// Transfers hub's money from the chanel to the hot-wallet
        /// </summary>
        HubCashout = 8,
        /// <summary>
        /// Money transfers from the client to the another client.
        /// For margin trading
        /// </summary>
        DirectTransferFromClient = 9
    }

    public interface IOffchainTransfer
    {
        string Id { get; }
        string ClientId { get; }
        string AssetId { get; }
        decimal Amount { get; }
        bool Completed { get; }
        string OrderId { get; }
        DateTime CreatedDt { get; }
        string ExternalTransferId { get; }
        OffchainTransferType Type { get; }
        bool ChannelClosing { get; }
        bool Onchain { get; }
        bool IsChild { get; }
        string ParentTransferId { get; }
        string AdditionalDataJson { get; set; }
        string BlockchainHash { get; set; }
    }

    public interface IOffchainTransferRepository
    {
        Task<IOffchainTransfer> CreateTransfer(string transactionId, string clientId, string assetId, decimal amount, OffchainTransferType type, string externalTransferId, string orderId, bool channelClosing = false);
        Task<IOffchainTransfer> GetTransfer(string id);
        Task CompleteTransfer(string transferId, bool? onchain = null, string blockchainHash = null);
        Task UpdateTransfer(string transferId, string toString, bool closing = false, bool? onchain = null);
        Task<IEnumerable<IOffchainTransfer>> GetTransfersByDate(OffchainTransferType type, DateTimeOffset from, DateTimeOffset to);
    }
}
