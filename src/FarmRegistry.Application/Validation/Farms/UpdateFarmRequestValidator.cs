using FluentValidation;
using FarmRegistry.Application.Contracts.Farms;

namespace FarmRegistry.Application.Validation.Farms;

public sealed class UpdateFarmRequestValidator : AbstractValidator<UpdateFarmRequest>
{
    public UpdateFarmRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.City)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.State)
            .NotEmpty()
            .Length(2)
            .Matches("^[A-Za-z]{2}$")
            .WithMessage("State deve ter 2 letras (UF).");
    }
}
