using FluentValidation.TestHelper;
using FarmRegistry.Application.Contracts.Fields;
using FarmRegistry.Application.Validation.Fields;
using FarmRegistry.Domain.Entities;
using Xunit;

namespace FarmRegistry.Application.Tests.Validation.Fields;

public sealed class CreateFieldRequestValidatorTests
{
    private readonly CreateFieldRequestValidator _validator = new();

    private static readonly IReadOnlyCollection<FieldBoundaryPointRequest> ValidBoundary =
    [
        new FieldBoundaryPointRequest(-21.201, -47.801),
        new FieldBoundaryPointRequest(-21.202, -47.802),
        new FieldBoundaryPointRequest(-21.203, -47.803)
    ];

    [Fact]
    public void Should_pass_when_valid()
    {
        var model = new CreateFieldRequest(
            Guid.NewGuid(),
            "TALHAO-01",
            "Talhão 01",
            12.5m,
            "Milho",
            ValidBoundary,
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
            "Milho",
            ValidBoundary,
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
            "Milho",
            ValidBoundary,
            (FieldStatus)0
        );

        _validator.TestValidate(model).ShouldHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void Should_fail_when_boundary_has_less_than_3_points()
    {
        var model = new CreateFieldRequest(
            Guid.NewGuid(),
            "TALHAO-01",
            "Talhão 01",
            10m,
            "Milho",
            [
                new FieldBoundaryPointRequest(-21.201, -47.801),
                new FieldBoundaryPointRequest(-21.202, -47.802)
            ],
            FieldStatus.Normal
        );

        _validator.TestValidate(model).ShouldHaveValidationErrorFor(x => x.BoundaryPoints);
    }
}
