using AutoMapper;
using FarmRegistry.Application.Mapping;
using Xunit;

namespace FarmRegistry.Application.Tests.Mapping;

public sealed class AutoMapperConfigurationTests
{
    [Fact]
    public void FarmRegistryProfile_should_be_valid()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<FarmRegistryProfile>());
        config.AssertConfigurationIsValid();
    }
}
