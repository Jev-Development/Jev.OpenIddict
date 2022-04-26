using AutoMapper;
using Jev.OpenIddict.Domain.Dto.Application;
using OpenIddict.EntityFrameworkCore.Models;

namespace Jev.OpenIddict.Core.Mappers
{
    public class ApplicationProfile : Profile
    {
        public ApplicationProfile()
        {
            CreateMap<OpenIddictEntityFrameworkCoreApplication, ApplicationViewDto>()
                .ForMember(dest => dest.Name, options => options.MapFrom(src => src.DisplayName))
                .ForMember(dest => dest.Name, options => options.MapFrom(src => src.ClientId));
        }
    }
}
