using Lykke.blue.Service.ReferralLinks.Core.Assets;

namespace Lykke.blue.Service.ReferralLinks.Models
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
