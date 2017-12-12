using AutoMapper;
using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using Lykke.blue.Service.ReferralLinks.Responses;

namespace Lykke.blue.Service.ReferralLinks
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<IReferralLink, GetReferralLinkResponse>().ForMember(x=>x.SenderClientId, v=> v.UseValue("")).ForMember(y=>y.SenderOffchainTransferId, z=>z.UseValue(""));            
            CreateMap<IReferralLinksStatistics, GetReferralLinksStatisticsBySenderIdResponse>();
        }
    }
}
