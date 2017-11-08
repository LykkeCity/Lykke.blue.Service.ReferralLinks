using AutoMapper;
using Lykke.Service.ReferralLinks.AzureRepositories.ReferralLink;
using Lykke.Service.ReferralLinks.Core.Domain.ReferralLink;
using Lykke.Service.ReferralLinks.Core.Domain.Requests;
using Lykke.Service.ReferralLinks.Requests;
using Lykke.Service.ReferralLinks.Responses;
using Lykke.Service.ReferralLinks.Services.Domain;

namespace Lykke.Service.ReferralLinks
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            //CreateMap<IReferralLink, CreateReferralLinkRequest>();
            CreateMap<IReferralLink, CreateReferralLinkResponse>();
            CreateMap<IReferralLink, GetReferralLinkResponse>();            
            CreateMap<IReferralLinksStatistics, GetReferralLinksStatisticsBySenderIdResponse>();
            CreateMap<MoneyTransferReferralLinkRequest, ReferralLink>();
            CreateMap<InvitationReferralLinkRequest, ReferralLink>();
            //CreateMap<IReferralLink, RequestMoneyTransferReferralLink>();
        }
    }
}
