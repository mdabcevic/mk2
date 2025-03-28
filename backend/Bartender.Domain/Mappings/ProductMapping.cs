using Bartender.Data.Models;
using AutoMapper;
using Bartender.Domain.DTO.Products;
using Bartender.Domain.DTO.Places;
using Bartender.Domain.DTO.MenuItems;

namespace Bartender.Domain.Mappings;

public class ProductMapping : Profile
{
    public ProductMapping()
    {
        CreateMap<Products, ProductBaseDto>()
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.Name));

        CreateMap<Products, ProductDto>()
        .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category));

        CreateMap<ProductCategory, ProductCategoryDto>();

        CreateMap<ProductCategory, GroupedProductsDto>()
            .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.Products))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Name));

        CreateMap<UpsertProductDto, Products>();
    }
}
