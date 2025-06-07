using AutoMapper;
using Bartender.Data.Models;
using Bartender.Domain.DTO.Staff;

namespace Bartender.Domain.Mappings;

public class StaffProfile : Profile
{
    public StaffProfile()
    {
        CreateMap<UpsertStaffDto, Staff>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
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