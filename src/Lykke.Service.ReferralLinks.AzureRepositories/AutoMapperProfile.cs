﻿using AutoMapper;
using Lykke.Service.ReferralLinks.AzureRepositories.DTOs;
using Lykke.Service.ReferralLinks.AzureRepositories.ReferralLink;
using Lykke.Service.ReferralLinks.Core.Domain.ReferralLink;
using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Lykke.Service.ReferralLinks.AzureRepositories
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            //To entities
            CreateMap<IReferralLink, ReferralLinkEntity>()
                .ForMember(dest => dest.RecipientType, opt => opt.MapFrom(src => src.RecipientType.ToString()))
                .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State.ToString()));

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

            //From entities
            CreateMap<ReferralLinkEntity, ReferralLinkDto>()
                .ForMember(dest => dest.RecipientType, opt => opt.MapFrom(src => Enum.Parse<RecipientType>(src.RecipientType)))
                .ForMember(dest => dest.State, opt => opt.MapFrom(src => Enum.Parse<ReferralLinkState>(src.State)));
        }
    }
}
