﻿using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using Rinkudesu.Services.Links.Models;

namespace Rinkudesu.Services.Links.DataTransferObjects
{
    [ExcludeFromCodeCoverage]
    public class LinkMappingProfile : Profile
    {
        public LinkMappingProfile()
        {
            CreateMap<Link, LinkDto>();
            CreateMap<LinkDto, Link>().ForMember(m => m.Id, options => options.Ignore())
                .ForMember(m => m.CreationDate, options => options.Ignore())
                .ForMember(m => m.LastUpdate, options => options.Ignore())
                .ForMember(m => m.CreatingUserId, options => options.Ignore());
        }
    }
}