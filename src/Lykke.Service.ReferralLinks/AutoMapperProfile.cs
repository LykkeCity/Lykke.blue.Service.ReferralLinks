using AutoMapper;
using Lykke.Service.ReferralLinks.AzureRepositories.ReferralLink;
using Lykke.Service.ReferralLinks.Core.Domain.ReferralLink;
using Lykke.Service.ReferralLinks.Requests;
using Lykke.Service.ReferralLinks.Responses;

namespace Lykke.Service.ReferralLinks
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<IReferralLink, CreateReferralLinkRequest>();
            CreateMap<IReferralLink, CreateReferralLinkResponse>();
            CreateMap<IReferralLink, GetReferralLinkResponse>();            
            CreateMap<IReferralLinksStatistics, GetReferralLinksStatisticsBySenderIdResponse>();            
            CreateMap<IReferralLink, RequestMoneyTransferReferralLink>();
        }
    }
}
