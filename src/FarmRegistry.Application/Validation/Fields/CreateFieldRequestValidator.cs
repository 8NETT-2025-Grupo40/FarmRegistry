using FarmRegistry.Application.Contracts.Fields;
using FarmRegistry.Domain.Entities;
using FluentValidation;

namespace FarmRegistry.Application.Validation.Fields;

public sealed class CreateFieldRequestValidator : AbstractValidator<CreateFieldRequest>
{
    private static readonly string[] AllowedStatuses = ["Normal", "AlertaSeca"];

    public CreateFieldRequestValidator()
    {
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

        RuleFor(x => x.Status)
            .IsInEnum()
            .NotEqual((FieldStatus)0); // protege caso venha 0 por default
    }
}
