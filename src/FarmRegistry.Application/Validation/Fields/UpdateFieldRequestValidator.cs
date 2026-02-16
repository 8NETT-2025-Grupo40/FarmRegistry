using FarmRegistry.Application.Contracts.Fields;
using FarmRegistry.Domain.Entities;
using FluentValidation;

namespace FarmRegistry.Application.Validation.Fields;

public sealed class UpdateFieldRequestValidator : AbstractValidator<UpdateFieldRequest>
{
    public UpdateFieldRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.FarmId)
            .NotEmpty();

        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.AreaHectares)
            .GreaterThan(0);

        RuleFor(x => x.CropName)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.BoundaryPoints)
            .NotNull()
            .Must(points => points.Count >= 3)
            .WithMessage("A delimitação do talhão deve ter pelo menos 3 pontos.");

        RuleForEach(x => x.BoundaryPoints)
            .ChildRules(point =>
            {
                point.RuleFor(p => p.Latitude)
                    .InclusiveBetween(-90, 90);

                point.RuleFor(p => p.Longitude)
                    .InclusiveBetween(-180, 180);
            });

        RuleFor(x => x.Status)
            .IsInEnum()
            .NotEqual((FieldStatus)0); // protege caso venha 0 por default
    }
}
