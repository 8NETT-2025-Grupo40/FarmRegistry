using FluentValidation.TestHelper;
using FarmRegistry.Application.Contracts.Fields;
using FarmRegistry.Application.Validation.Fields;
using FarmRegistry.Domain.Entities;
using Xunit;

namespace FarmRegistry.Application.Tests.Validation.Fields;

public sealed class CreateFieldRequestValidatorTests
{
    private readonly CreateFieldRequestValidator _validator = new();

    [Fact]
    public void Should_pass_when_valid()
    {
        var model = new CreateFieldRequest(
            Guid.NewGuid(),
            "TALHAO-01",
            "Talhão 01",
            12.5m,
            FieldStatus.Normal
        );

        _validator.TestValidate(model).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_fail_when_area_invalid()
    {
        var model = new CreateFieldRequest(
            Guid.NewGuid(),
            "TALHAO-01",
            "Talhão 01",
            0m,
            FieldStatus.Normal
        );

        _validator.TestValidate(model).ShouldHaveValidationErrorFor(x => x.AreaHectares);
    }

    [Fact]
    public void Should_fail_when_status_zero()
    {
        var model = new CreateFieldRequest(
            Guid.NewGuid(),
            "TALHAO-01",
            "Talhão 01",
            10m,
            (FieldStatus)0
        );

        _validator.TestValidate(model).ShouldHaveValidationErrorFor(x => x.Status);
    }
}
