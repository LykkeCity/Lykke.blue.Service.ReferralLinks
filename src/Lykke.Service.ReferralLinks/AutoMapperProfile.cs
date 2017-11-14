using AutoMapper;
using Lykke.Blue.Service.ReferralLinks.AzureRepositories.ReferralLink;
using Lykke.Blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using Lykke.Blue.Service.ReferralLinks.Core.Domain.Requests;
using Lykke.Blue.Service.ReferralLinks.Requests;
using Lykke.Blue.Service.ReferralLinks.Responses;
using Lykke.Blue.Service.ReferralLinks.Services.Domain;

namespace Lykke.Blue.Service.ReferralLinks
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
