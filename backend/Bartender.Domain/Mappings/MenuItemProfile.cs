using AutoMapper;
using Bartender.Data.Models;
using Bartender.Domain.DTO.MenuItem;

namespace Bartender.Domain.Mappings;

public class MenuItemProfile : Profile
{
    public MenuItemProfile() {
        CreateMap<MenuItem, MenuItemDto>()
            .ForMember(dest => dest.Place, opt => opt.MapFrom(src => src.Place))
            .ForMember(dest => dest.Product, opt => opt.MapFrom(src => src.Product));

        CreateMap<MenuItem, MenuItemBaseDto>()
             .ForMember(dest => dest.Product, opt => opt.MapFrom(src => src.Product))
             .ForMember(dest => dest.FormattedPrice, opt => opt.Ignore());

        CreateMap<UpsertMenuItemDto, MenuItem>();

        CreateMap<MenuItem, UpsertMenuItemDto>();
    }
}
