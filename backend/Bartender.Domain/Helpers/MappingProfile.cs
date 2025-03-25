using Bartender.Data.Models;
using AutoMapper;
using Bartender.Domain.DTO.Products;
using Bartender.Domain.DTO.Places;
using Bartender.Domain.DTO.MenuItems;

namespace Bartender.Domain.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Products, ProductsBaseDTO>()
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.Name));

            CreateMap<Products, ProductsDTO>()
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category));
            //.ForMember(dest => dest.Menu, opt => opt.MapFrom(src => src.MenuItems));

            CreateMap<ProductCategory, ProductCategoryDTO>();

            CreateMap<ProductCategory, GroupedProductsDTO>()
                .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.Products))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Name));

            CreateMap<MenuItems, MenuItemsDTO>()
                .ForMember(dest => dest.Place, opt => opt.MapFrom(src => src.Place))
                .ForMember(dest => dest.Product, opt => opt.MapFrom(src => src.Product));

            CreateMap<MenuItems, MenuItemsBaseDTO>()
                .ForMember(dest => dest.Product, opt => opt.MapFrom(src => src.Product));

            CreateMap<UpsertProductDTO, Products>();
            CreateMap<UpsertMenuItemDTO, MenuItems>();

            CreateMap<Places, GroupedMenusDTO>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.MenuItems));

            CreateMap<Places, PlaceDTO>()
                .ForMember(dest => dest.BusinessName, opt => opt.MapFrom(src => src.Business.Name));
        }
    }
}
