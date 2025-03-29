using AutoMapper;
using Bartender.Data.Models;
using Bartender.Domain.DTO.MenuItems;

namespace Bartender.Domain.Mappings;

public class MenuItemProfile : Profile
{
    public MenuItemProfile() {
        CreateMap<MenuItems, MenuItemDto>()
            .ForMember(dest => dest.Place, opt => opt.MapFrom(src => src.Place))
            .ForMember(dest => dest.Product, opt => opt.MapFrom(src => src.Product));

        CreateMap<MenuItems, MenuItemBaseDto>()
             .ForMember(dest => dest.Product, opt => opt.MapFrom(src => src.Product))
             .ForMember(dest => dest.FormattedPrice, opt => opt.Ignore());

        CreateMap<UpsertMenuItemDto, MenuItems>();

        CreateMap<MenuItems, UpsertMenuItemDto>();
    }
}
