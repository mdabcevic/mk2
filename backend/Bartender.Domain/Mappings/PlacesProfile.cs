using AutoMapper;
using Bartender.Data.Models;
using Bartender.Domain.DTO;

namespace Bartender.Domain.Mappings
{
    internal class PlacesProfile : Profile
    {
        public PlacesProfile()
        {
            CreateMap<Places, PlaceDto>()
            .ForMember(dest => dest.BusinessName, opt => opt.MapFrom(src => src.Business!.Name))
            .ForMember(dest => dest.CityName, opt => opt.MapFrom(src => src.City!.Name))
            .ForMember(dest => dest.WorkHours, opt => opt.MapFrom(src => $"{src.OpensAt:hh\\:mm} - {src.ClosesAt:hh\\:mm}"))
            .ForMember(dest => dest.MenuItems, opt => opt.MapFrom(src => src.MenuItems));

            CreateMap<UpsertPlaceDto, Places>()
            .ForMember(dest => dest.OpensAt,
                opt =>
                {
                    opt.PreCondition(src => !string.IsNullOrEmpty(src.OpensAt));
                    opt.MapFrom(src => TimeOnly.Parse(src.OpensAt!));
                })
            .ForMember(dest => dest.ClosesAt,
                opt =>
                {
                    opt.PreCondition(src => !string.IsNullOrEmpty(src.ClosesAt));
                    opt.MapFrom(src => TimeOnly.Parse(src.ClosesAt!));
                }
            );
        }
    }
}
