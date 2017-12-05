namespace Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain
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
}