using FluentValidation;
using Lykke.Service.ReferralLinks.Core.Services;
using Lykke.Service.ReferralLinks.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.ReferralLinks.Modules.Validation
{
    public class ClaimGiftCoinsValidationModel : AbstractValidator<TransferFromLykkeWalletToRecipient>
    {
        //private readonly IReferralLinksService _referralLinksService;

        public ClaimGiftCoinsValidationModel(/*IReferralLinksService referralLinksService*/)
        {
            //_referralLinksService = referralLinksService;

            RuleFor(reg => reg.ReferalLinkId).NotNull().WithMessage("ReferalLinkId not specified");
            RuleFor(reg => reg.RecipientClientId).NotNull().WithMessage("RecipientClientId not specified");
            //RuleFor(reg => reg.ReferalLinkId).Must(RefLinkExists).WithMessage(reg => $"RefLink with id {reg.ReferalLinkId} not found.");
        }

        //private bool RefLinkExists(string value)
        //{
        //    return _referralLinksService.GetReferralLinkById(value) != null;
        //}
    }
}
