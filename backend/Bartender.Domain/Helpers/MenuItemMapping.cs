using AutoMapper;
using Bartender.Data.Models;
using Bartender.Domain.DTO.MenuItems;

namespace Bartender.Domain.Helpers
{
    public class MenuItemMapping : Profile
    {
        public MenuItemMapping() {
            CreateMap<MenuItems, MenuItemsDTO>()
                .ForMember(dest => dest.Place, opt => opt.MapFrom(src => src.Place))
                .ForMember(dest => dest.Product, opt => opt.MapFrom(src => src.Product));

            CreateMap<MenuItems, MenuItemsBaseDTO>()
                .ForMember(dest => dest.Product, opt => opt.MapFrom(src => src.Product));

            CreateMap<UpsertMenuItemDTO, MenuItems>();
        }
    }
}
