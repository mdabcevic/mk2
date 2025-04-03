using AutoMapper;
using Bartender.Data.Models;
using Bartender.Domain.DTO.Orders;

namespace Bartender.Domain.Mappings;

public class OrderProfile : Profile
{
   public OrderProfile() {
        CreateMap<UpsertOrderDto, Orders>();
        CreateMap<UpsertOrderMenuItemDto, ProductsPerOrder>();
        CreateMap<Orders, OrderBaseDto>();
        CreateMap<Orders, OrderDto>();
        CreateMap<ProductsPerOrder, OrderItemsDto>();

    }
}
