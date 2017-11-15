using AutoMapper;
using Lykke.blue.Service.ReferralLinks.AzureRepositories.ReferralLink;
using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using Lykke.blue.Service.ReferralLinks.Core.Domain.Requests;
using Lykke.blue.Service.ReferralLinks.Requests;
using Lykke.blue.Service.ReferralLinks.Responses;
using Lykke.blue.Service.ReferralLinks.Services.Domain;

namespace Lykke.blue.Service.ReferralLinks
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            //CreateMap<IReferralLink, CreateReferralLinkRequest>();
            //CreateMap<IReferralLink, CreateReferralLinkResponse>();
            CreateMap<IReferralLink, GetReferralLinkResponse>();            
            CreateMap<IReferralLinksStatistics, GetReferralLinksStatisticsBySenderIdResponse>();
            //CreateMap<MoneyTransferReferralLinkRequest, ReferralLink>();
            //CreateMap<InvitationReferralLinkRequest, ReferralLink>();
            //CreateMap<IReferralLink, RequestMoneyTransferReferralLink>();
        }
    }
}
