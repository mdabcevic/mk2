using AutoMapper;
using Bartender.Data.Models;
using Bartender.Domain.DTO.Order;

namespace Bartender.Domain.Mappings;

public class OrderProfile : Profile
{
   public OrderProfile() {
        CreateMap<UpsertOrderDto, Order>();

        CreateMap<UpsertOrderMenuItemDto, ProductPerOrder>();

        CreateMap<Order, OrderBaseDto>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("dd.MM.yyyy HH:mm")));

        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Products))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("dd.MM.yyyy HH:mm")))
            .ForMember(dest => dest.Table, opt => opt.MapFrom(src => $"{src.Table.Label} ({src.TableId})"));

        CreateMap<ProductPerOrder, OrderItemsDto>()
            .ForMember(dest => dest.MenuItem, opt => opt.MapFrom(src => $"{src.MenuItem.Product.Name} ({src.MenuItem.Product.Volume})"));
    }
}
