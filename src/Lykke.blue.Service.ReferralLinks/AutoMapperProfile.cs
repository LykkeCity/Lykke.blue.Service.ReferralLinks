using AutoMapper;
using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using Lykke.blue.Service.ReferralLinks.Responses;

namespace Lykke.blue.Service.ReferralLinks
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<IReferralLink, GetReferralLinkResponse>();          
            CreateMap<IReferralLinksStatistics, GetReferralLinksStatisticsBySenderIdResponse>();
        }
    }
}
