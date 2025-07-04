using FluentValidation;
using NPV.Shared.Models;

namespace NPV.Shared.Validators
{
    public class NpvRequestValidator : AbstractValidator<NpvRequest>
    {
        public NpvRequestValidator()
        {
            RuleFor(x => x.CashFlows)
                .NotNull().WithMessage("CashFlows are required.")
                .Must(cf => cf.Any()).WithMessage("CashFlows must contain at least one value.");

            RuleFor(x => x.LowerBound)
                .LessThanOrEqualTo(x => x.UpperBound)
                .WithMessage("LowerBound must not exceed UpperBound.");

            RuleFor(x => x.Increment)
                .GreaterThan(0).WithMessage("Increment must be positive.");
        }
    }
}
