using AutoMapper;
using Lykke.Blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using Lykke.Blue.Service.ReferralLinks.Services.Domain;

namespace Lykke.Blue.Service.ReferralLinks.Services
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<IReferralLink, ReferralLink>();
        }
    }
}
