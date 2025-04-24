using AutoMapper;
using Bartender.Data.Models;
using Bartender.Domain.DTO.Business;

namespace Bartender.Domain.Mappings;

public class BusinessProfile : Profile
{
    public BusinessProfile()
    {
        // Entity to DTO
        CreateMap<Business, BusinessDto>();

        // DTO to Entity (Insert/Update)
        CreateMap<UpsertBusinessDto, Business>();
    }
}
