using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Blue.Service.ReferralLinks.Core.Domain.WalletCredentials
{
    public interface IWalletCredentials
    {
        string ClientId { get; }
        string Address { get; }
        string PublicKey { get; }
        string PrivateKey { get; }
        string MultiSig { get; }
        string ColoredMultiSig { get; }
        bool PreventTxDetection { get; }
        string EncodedPrivateKey { get; }

        string BtcConvertionWalletPrivateKey { get; set; }
        string BtcConvertionWalletAddress { get; set; }

        string EthConversionWalletAddress { get; set; }
        string EthAddress { get; set; }
        string EthPublicKey { get; set; }

        string SolarCoinWalletAddress { get; set; }

        string ChronoBankContract { get; set; }

        string QuantaContract { get; set; }
    }
}
