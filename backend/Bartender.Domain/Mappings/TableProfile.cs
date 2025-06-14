﻿using AutoMapper;
using Bartender.Data.Models;
using Bartender.Domain.DTO.Table;

namespace Bartender.Domain.Mappings;

public class TableProfile : Profile
{
    public TableProfile()
    {
        CreateMap<Table, TableDto>()
            .IncludeBase<Table, BaseTableDto>()
            .ForMember(dest => dest.Token, opt => opt.MapFrom(src => src.QrSalt));

        CreateMap<Table, TableScanDto>()
            .ForMember(dest => dest.GuestToken, opt => opt.Ignore()); // set manually

        CreateMap<UpsertTableDto, Table>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PlaceId, opt => opt.Ignore())
            .ForMember(dest => dest.QrSalt, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore());

        CreateMap<Table, UpsertTableDto>();
        CreateMap<Table, BaseTableDto>();
    }
}
