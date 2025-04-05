using AutoMapper;
using Bartender.Data.Models;
using Bartender.Domain.DTO.Orders;

namespace Bartender.Domain.Mappings;

public class OrderProfile : Profile
{
   public OrderProfile() {
        CreateMap<UpsertOrderDto, Orders>();

        CreateMap<UpsertOrderMenuItemDto, ProductsPerOrder>();

        CreateMap<Orders, OrderBaseDto>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("dd.MM.yyyy HH:mm")))
            .ForMember(dest => dest.Table, opt => opt.MapFrom(src => src.Table.Label));

        CreateMap<Orders, OrderDto>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Products))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("dd.MM.yyyy HH:mm")))
            .ForMember(dest => dest.Table, opt => opt.MapFrom(src => $"{src.Table.Label} ({src.TableId})"));

        CreateMap<ProductsPerOrder, OrderItemsDto>()
            .ForMember(dest => dest.MenuItem, opt => opt.MapFrom(src => $"{src.MenuItem.Product.Name} ({src.MenuItem.Product.Volume})"));
    }
}
