using Bartender.Data.Models;
using AutoMapper;
using Bartender.Domain.DTO.Product;

namespace Bartender.Domain.Mappings;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductBaseDto>()
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.Name));

        CreateMap<Product, ProductDto>()
        .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
        .ForMember(dest => dest.Exclusive, opt => opt.MapFrom(src => src.BusinessId != null));

        CreateMap<ProductCategory, ProductCategoryDto>();

        CreateMap<ProductCategory, GroupedProductsDto>()
            .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.Products))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Name));

        CreateMap<UpsertProductDto, Product>();
    }
}
