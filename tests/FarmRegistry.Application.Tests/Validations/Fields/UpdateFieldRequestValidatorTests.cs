using FluentValidation.TestHelper;
using FarmRegistry.Application.Contracts.Fields;
using FarmRegistry.Application.Validation.Fields;
using FarmRegistry.Domain.Entities;
using Xunit;

namespace FarmRegistry.Application.Tests.Validation.Fields;

public sealed class UpdateFieldRequestValidatorTests
{
    private readonly UpdateFieldRequestValidator _validator = new();

    private static readonly IReadOnlyCollection<FieldBoundaryPointRequest> ValidBoundary =
    [
        new FieldBoundaryPointRequest(-21.201, -47.801),
        new FieldBoundaryPointRequest(-21.202, -47.802),
        new FieldBoundaryPointRequest(-21.203, -47.803)
    ];

    [Fact]
    public void Should_pass_when_valid()
    {
        var model = new UpdateFieldRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TALHAO-02",
            "Talhão 02",
            8m,
            "Soja",
            ValidBoundary,
            FieldStatus.AlertaSeca
        );

        _validator.TestValidate(model).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_fail_when_id_empty()
    {
        var model = new UpdateFieldRequest(
            Guid.Empty,
            Guid.NewGuid(),
            "TALHAO-02",
            "Talhão 02",
            8m,
            "Soja",
            ValidBoundary,
            FieldStatus.Normal
        );

        _validator.TestValidate(model).ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Should_fail_when_status_zero()
    {
        var model = new UpdateFieldRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TALHAO-02",
            "Talhão 02",
            8m,
            "Soja",
            ValidBoundary,
            (FieldStatus)0
        );

        _validator.TestValidate(model).ShouldHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void Should_fail_when_crop_is_empty()
    {
        var model = new UpdateFieldRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TALHAO-02",
            "Talhão 02",
            8m,
            string.Empty,
            ValidBoundary,
            FieldStatus.Normal
        );

        _validator.TestValidate(model).ShouldHaveValidationErrorFor(x => x.CropName);
    }
}
