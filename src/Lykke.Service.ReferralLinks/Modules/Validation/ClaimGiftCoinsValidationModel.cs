using FluentValidation;
using Lykke.Blue.Service.ReferralLinks.Core.Services;
using Lykke.Blue.Service.ReferralLinks.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Blue.Service.ReferralLinks.Modules.Validation
{
    public class ClaimReferralLinkRequestValidationModel : AbstractValidator<ClaimReferralLinkRequest>
    {
        //private readonly IReferralLinksService _referralLinksService;

        public ClaimReferralLinkRequestValidationModel(/*IReferralLinksService referralLinksService*/)
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
