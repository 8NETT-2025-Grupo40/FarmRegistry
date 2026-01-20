using AutoMapper;
using FarmRegistry.Application.Contracts.Farms;
using FarmRegistry.Application.Contracts.Fields;
using FarmRegistry.Domain.Entities;

namespace FarmRegistry.Application.Mapping;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Farm, FarmResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.FarmId));

        CreateMap<Field, FieldResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.FieldId))
            .ForMember(dest => dest.AreaHectares, opt => opt.MapFrom(src => (decimal)src.AreaHectares));
    }
}