using Lykke.blue.Service.ReferralLinks.Client.AutorestClient.Models;

namespace Lykke.blue.Service.ReferralLinks.Client.Models
{
    class ClaimReferralLinkDto
    {
        public string TransactionRewardRecipient { get; set; }
        public string TransactionRewardSender { get; set; }
        public string SenderTransactionId { get; set; }

        public static ClaimReferralLinkDto Create(ClaimRefLinkResponse model)
        {
            return new ClaimReferralLinkDto
            {
                TransactionRewardRecipient = model.TransactionRewardRecipient,
                TransactionRewardSender = model.TransactionRewardRecipient,
                SenderTransactionId = model.SenderTransactionId
            };
        }
    }
}




