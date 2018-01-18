using FluentValidation;
using Lykke.blue.Service.ReferralLinks.Models;
// ReSharper disable UnusedMember.Global

namespace Lykke.blue.Service.ReferralLinks.Modules.Validation
{
    public class ClaimReferralLinkRequestValidationModel : AbstractValidator<ClaimReferralLinkRequest>
    {
        public ClaimReferralLinkRequestValidationModel()
        {
            RuleFor(reg => reg.RecipientClientId).NotNull().WithMessage("RecipientClientId not specified");
        }

    }
}
