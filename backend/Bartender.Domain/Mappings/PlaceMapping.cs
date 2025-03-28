using AutoMapper;
using Bartender.Data.Models;
using Bartender.Domain.DTO.MenuItems;
using Bartender.Domain.DTO.Places;

namespace Bartender.Domain.Mappings
{
    public class PlaceMapping : Profile
    {
        public PlaceMapping() {
            CreateMap<Places, GroupedPlaceMenuDto>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.MenuItems))
                .ForMember(dest => dest.Place, opt => opt.MapFrom(src => src));

            CreateMap<Places, PlaceDto>()
                .ForMember(dest => dest.BusinessName, opt => opt.MapFrom(src => src.Business.Name));
        }
    }
}
