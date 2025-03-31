using AutoMapper;
using Bartender.Data.Models;
using Bartender.Domain.DTO;

namespace Bartender.Domain.Mappings;

public class BusinessProfile : Profile
{
    public BusinessProfile()
    {
        // Entity to DTO
        CreateMap<Businesses, BusinessDto>();

        // DTO to Entity (Insert/Update)
        CreateMap<UpsertBusinessDto, Businesses>();
    }
}
