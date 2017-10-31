using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.ReferralLinks.Core.BitCoinApi.Models
{
    public enum TransferType
    {
        Common = 0,
        ToMarginAccount = 1,
        FromMarginAccount = 2,
        ToTrustedWallet = 3,
        FromTrustedWallet = 4
    }
}
