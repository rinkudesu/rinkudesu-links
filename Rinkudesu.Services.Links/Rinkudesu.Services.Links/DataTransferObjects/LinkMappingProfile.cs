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
            CreateMap<Link, LinkDto>();
            CreateMap<LinkCreateDto, Link>();
            CreateMap<LinkDto, Link>()
                .ForMember(m => m.Id, o => o.Ignore())
                .ForMember(m => m.CreationDate, o => o.Ignore())
                .ForMember(m => m.LastUpdate, o => o.Ignore())
                .ForMember(m => m.CreatingUserId, o => o.Ignore());
        }
    }
}
