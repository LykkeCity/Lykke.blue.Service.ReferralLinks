using Lykke.Service.ReferralLinks.Core.Assets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.ReferralLinks.Models
{
    public static class ModelConverter
    {
        public static Asset ConvertToServiceModel(this Lykke.Service.Assets.Client.Models.Asset src)
        {
            return new Asset
            {
                Id = src.Id,
                Name = src.Name,
                Accuracy = src.Accuracy,
                Symbol = src.Symbol,
                HideWithdraw = src.HideWithdraw,
                HideDeposit = src.HideDeposit,
                KycNeeded = src.KycNeeded,
                BankCardsDepositEnabled = src.BankCardsDepositEnabled,
                SwiftDepositEnabled = src.SwiftDepositEnabled,
                BlockchainDepositEnabled = src.BlockchainDepositEnabled,
                CategoryId = src.CategoryId
            };
        }
    }
}
