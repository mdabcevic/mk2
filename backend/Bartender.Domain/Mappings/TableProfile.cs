using AutoMapper;
using Bartender.Data.Models;
using Bartender.Domain.DTO;

namespace Bartender.Domain.Mappings;

public class TableProfile : Profile
{
    public TableProfile()
    {
        CreateMap<Tables, TableDto>()
            .ForMember(dest => dest.Token, opt => opt.MapFrom(src => src.QrSalt));

        CreateMap<UpsertTableDto, Tables>();
    }
}
