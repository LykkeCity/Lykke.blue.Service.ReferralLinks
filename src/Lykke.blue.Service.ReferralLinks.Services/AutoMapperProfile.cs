using AutoMapper;
using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using Lykke.blue.Service.ReferralLinks.Services.Domain;

namespace Lykke.blue.Service.ReferralLinks.Services
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<IReferralLink, ReferralLink>();
        }
    }
}
