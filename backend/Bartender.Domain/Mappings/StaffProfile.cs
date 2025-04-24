using AutoMapper;
using Bartender.Data.Models;
using Bartender.Domain.DTO.Staff;

namespace Bartender.Domain.Mappings;

public class StaffMappingProfile : Profile
{
    public StaffMappingProfile()
    {
        CreateMap<UpsertStaffDto, Staff>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Ensure ID is never set from DTO
            .ForMember(dest => dest.FullName, opt =>
                opt.MapFrom(src => $"{src.FirstName.Trim()} {src.LastName.Trim()}"))
            .ForMember(dest => dest.Password, opt =>
                opt.MapFrom(src => HashPassword(src.Password)));

        CreateMap<Staff, StaffDto>();
    }

    private static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}