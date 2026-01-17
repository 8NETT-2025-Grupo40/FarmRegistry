using FluentValidation.TestHelper;
using FarmRegistry.Application.Contracts.Farms;
using FarmRegistry.Application.Validation.Farms;
using Xunit;

namespace FarmRegistry.Application.Tests.Validation.Farms;

public sealed class UpdateFarmRequestValidatorTests
{
    private readonly UpdateFarmRequestValidator _validator = new();

    [Fact]
    public void Should_pass_when_valid()
    {
        var model = new UpdateFarmRequest(Guid.NewGuid(), "Fazenda B", "Campinas", "SP");
        _validator.TestValidate(model).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_fail_when_id_empty()
    {
        var model = new UpdateFarmRequest(Guid.Empty, "Fazenda B", "Campinas", "SP");
        _validator.TestValidate(model).ShouldHaveValidationErrorFor(x => x.Id);
    }
}
