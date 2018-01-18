using AutoMapper;
using Lykke.blue.Service.ReferralLinks.AzureRepositories.ReferralLink;
using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.blue.Service.ReferralLinks.AzureRepositories
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // To entities

            CreateMap<IReferralLink, ReferralLinkEntity>();
            CreateMap<IReferralLinkClaim, ReferralLinkClaimEntity>();

            ForAllMaps((map, cfg) =>
            {
                if (map.DestinationType.IsSubclassOf(typeof(TableEntity)))
                {
                    cfg.ForMember("ETag", opt => opt.Ignore());
                    cfg.ForMember("PartitionKey", opt => opt.Ignore());
                    cfg.ForMember("RowKey", opt => opt.Ignore());
                    cfg.ForMember("Timestamp", opt => opt.Ignore());
                }
            });
        }
    }
}
