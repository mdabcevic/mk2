using AutoMapper;
using Bartender.Data.Models;
using Bartender.Domain.DTO;

namespace Bartender.Domain.Mappings;

public class MenuItemProfile : Profile
{
    public MenuItemProfile()
    {
        CreateMap<MenuItems, MenuItemDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Product!.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));
            //.ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Product!.Category));
    }
}
