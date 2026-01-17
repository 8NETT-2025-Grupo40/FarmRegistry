using FluentValidation.TestHelper;
using FarmRegistry.Application.Contracts.Farms;
using FarmRegistry.Application.Validation.Farms;
using Xunit;

namespace FarmRegistry.Application.Tests.Validation.Farms;

public sealed class CreateFarmRequestValidatorTests
{
    private readonly CreateFarmRequestValidator _validator = new();

    [Fact]
    public void Should_pass_when_valid()
    {
        var model = new CreateFarmRequest("Fazenda A", "Ribeirão Preto", "SP");
        _validator.TestValidate(model).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_fail_when_state_invalid()
    {
        var model = new CreateFarmRequest("Fazenda A", "Ribeirão Preto", "SaoPaulo");
        _validator.TestValidate(model).ShouldHaveValidationErrorFor(x => x.State);
    }

    [Fact]
    public void Should_fail_when_name_empty()
    {
        var model = new CreateFarmRequest("", "Ribeirão Preto", "SP");
        _validator.TestValidate(model).ShouldHaveValidationErrorFor(x => x.Name);
    }
}
