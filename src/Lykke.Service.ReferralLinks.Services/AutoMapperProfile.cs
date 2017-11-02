using AutoMapper;
using Lykke.Service.ReferralLinks.Core.Domain.ReferralLink;
using Lykke.Service.ReferralLinks.Services.Domain;

namespace Lykke.Service.ReferralLinks.Services
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<IReferralLink, ReferralLink>();
        }
    }
}
