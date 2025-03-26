using Bartender.Data.Models;
using AutoMapper;
using Bartender.Domain.DTO.Products;
using Bartender.Domain.DTO.Places;
using Bartender.Domain.DTO.MenuItems;

namespace Bartender.Domain.Helpers
{
    public class ProductMapping : Profile
    {
        public ProductMapping()
        {
            CreateMap<Products, ProductsBaseDTO>()
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.Name));

            CreateMap<Products, ProductsDTO>()
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category));

            CreateMap<ProductCategory, ProductCategoryDTO>();

            CreateMap<ProductCategory, GroupedProductsDTO>()
                .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.Products))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Name));

            CreateMap<UpsertProductDTO, Products>();
        }
    }
}
