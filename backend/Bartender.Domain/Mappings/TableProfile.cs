using AutoMapper;
using Bartender.Data.Models;
using Bartender.Domain.DTO;

namespace Bartender.Domain.Mappings;

public class TableProfile : Profile
{
    public TableProfile()
    {
        CreateMap<Tables, TableDto>();
        CreateMap<UpsertTableDto, Tables>();
    }
}
