using System;
using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using Rinkudesu.Services.Links.DataTransferObjects.V1;
using Rinkudesu.Services.Links.Models;
#pragma warning disable 1591

namespace Rinkudesu.Services.Links.DataTransferObjects
{
    [ExcludeFromCodeCoverage]
    public class LinkMappingProfile : Profile
    {
        public LinkMappingProfile()
        {
            CreateMap<Link, LinkDto>().ForMember(m => m.LinkUrl, c => c.MapFrom(source => new Uri(source.LinkUrl, UriKind.RelativeOrAbsolute)));
            CreateMap<LinkDto, Link>().ForMember(m => m.Id, options => options.Ignore())
                .ForMember(m => m.CreationDate, options => options.Ignore())
                .ForMember(m => m.LastUpdate, options => options.Ignore())
                .ForMember(m => m.CreatingUserId, options => options.Ignore());
        }
    }
}
