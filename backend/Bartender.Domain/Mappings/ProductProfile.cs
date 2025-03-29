using Bartender.Data.Models;
using AutoMapper;
using Bartender.Domain.DTO.Products;

namespace Bartender.Domain.Mappings;

public class ProductProfile : Profile
{
    public ProductProfile()
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
