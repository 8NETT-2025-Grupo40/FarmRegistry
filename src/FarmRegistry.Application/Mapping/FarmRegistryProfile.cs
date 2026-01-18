using AutoMapper;
using FarmRegistry.Application.Contracts.Farms;
using FarmRegistry.Application.Contracts.Fields;
using FarmRegistry.Domain.Entities;

namespace FarmRegistry.Application.Mapping;

public sealed class FarmRegistryProfile : Profile
{
    public FarmRegistryProfile()
    {
        CreateMap<Farm, FarmResponse>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.FarmId));

        CreateMap<Field, FieldResponse>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.FieldId))
            .ForMember(d => d.AreaHectares, opt => opt.MapFrom(s => (decimal)s.AreaHectares))
            .ForMember(d => d.StatusUpdatedAt, opt => opt.MapFrom(s => (DateTime?)s.StatusUpdatedAt));
    }
}
