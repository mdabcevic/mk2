using AutoMapper;
using Bartender.Data.Models;
using Bartender.Domain.DTO;
using Bartender.Domain.DTO.MenuItems;
using Bartender.Domain.DTO.Products;

namespace Bartender.Domain.Mappings;

public class PlacesProfile : Profile
{
    public PlacesProfile()
    {
        CreateMap<Places, PlaceDto>()
        .ForMember(dest => dest.BusinessName, opt => opt.MapFrom(src => src.Business!.Name))
        .ForMember(dest => dest.CityName, opt => opt.MapFrom(src => src.City!.Name))
        .ForMember(dest => dest.WorkHours, opt => opt.MapFrom(src => $"{src.OpensAt:hh\\:mm} - {src.ClosesAt:hh\\:mm}"));

        CreateMap<Places, PlaceWithMenuDto>()
        .ForMember(dest => dest.BusinessName, opt => opt.MapFrom(src => src.Business!.Name))
        .ForMember(dest => dest.CityName, opt => opt.MapFrom(src => src.City!.Name))
        .ForMember(dest => dest.WorkHours, opt => opt.MapFrom(src => $"{src.OpensAt:hh\\:mm} - {src.ClosesAt:hh\\:mm}"))
        .ForMember(dest => dest.Menu, opt => opt.MapFrom(src => src.MenuItems));

        CreateMap<Places, GroupedPlaceMenuDto>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.MenuItems))
            .ForMember(dest => dest.Place, opt => opt.MapFrom(src => src));

        CreateMap<InsertPlaceDto, Places>()
        .ForMember(dest => dest.OpensAt,
            opt =>
            {
                opt.PreCondition(src => !string.IsNullOrEmpty(src.OpensAt));
                opt.MapFrom(src => TimeOnly.Parse(src.OpensAt!)); //TODO: custom parser
            })
        .ForMember(dest => dest.ClosesAt,
            opt =>
            {
                opt.PreCondition(src => !string.IsNullOrEmpty(src.ClosesAt));
                opt.MapFrom(src => TimeOnly.Parse(src.ClosesAt!)); //TODO: custom parser
            }
        );

        CreateMap<UpdatePlaceDto, Places>()
        .ForMember(dest => dest.OpensAt,
            opt =>
            {
                opt.PreCondition(src => !string.IsNullOrEmpty(src.OpensAt));
                opt.MapFrom(src => TimeOnly.Parse(src.OpensAt!)); //TODO: custom parser
            })
        .ForMember(dest => dest.ClosesAt,
            opt =>
            {
                opt.PreCondition(src => !string.IsNullOrEmpty(src.ClosesAt));
                opt.MapFrom(src => TimeOnly.Parse(src.ClosesAt!)); //TODO: custom parser
            }
        );
    }
}
