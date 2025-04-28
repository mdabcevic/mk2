using AutoMapper;
using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.DTO.MenuItem;
using Bartender.Domain.DTO.Place;

namespace Bartender.Domain.Mappings;

public class PlaceProfile : Profile
{
    public PlaceProfile()
    {
        CreateMap<Place, PlaceDto>()
        .ForMember(dest => dest.BusinessName, opt => opt.MapFrom(src => src.Business!.Name))
        .ForMember(dest => dest.CityName, opt => opt.MapFrom(src => src.City!.Name))
        .ForMember(dest => dest.WorkHours, opt => opt.MapFrom(src => $"{src.OpensAt:hh\\:mm} - {src.ClosesAt:hh\\:mm}"));

        CreateMap<Place, PlaceWithMenuDto>()
        .ForMember(dest => dest.BusinessName, opt => opt.MapFrom(src => src.Business!.Name))
        .ForMember(dest => dest.CityName, opt => opt.MapFrom(src => src.City!.Name))
        .ForMember(dest => dest.WorkHours, opt => opt.MapFrom(src => $"{src.OpensAt:hh\\:mm} - {src.ClosesAt:hh\\:mm}"))
        .ForMember(dest => dest.FreeTablesCount, opt => opt.MapFrom(src => src.Tables!.Count(t => t.Status == TableStatus.empty && !t.IsDisabled)))
        .ForMember(dest => dest.Menu, opt => opt.MapFrom(src => src.MenuItems));

        CreateMap<Place, GroupedPlaceMenuDto>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.MenuItems))
            .ForMember(dest => dest.Place, opt => opt.MapFrom(src => src));

        CreateMap<InsertPlaceDto, Place>()
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

        CreateMap<UpdatePlaceDto, Place>()
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
