using FluentValidation;
using Lykke.blue.Service.ReferralLinks.Models;

namespace Lykke.blue.Service.ReferralLinks.Modules.Validation
{
    public class ClaimReferralLinkRequestValidationModel : AbstractValidator<ClaimReferralLinkRequest>
    {
        public ClaimReferralLinkRequestValidationModel()
        {
            RuleFor(reg => reg.ReferalLinkId).NotNull().WithMessage("ReferalLinkId not specified");
            RuleFor(reg => reg.RecipientClientId).NotNull().WithMessage("RecipientClientId not specified");
        }

    }
}
