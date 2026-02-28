using FluentValidation;

namespace EA.Application.Features.PlacementTests.SubmitPlacementTest;

public class SubmitPlacementTestCommandValidator : AbstractValidator<SubmitPlacementTestCommand>
{
    public SubmitPlacementTestCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Answers).NotEmpty().WithMessage("At least one answer is required.");
    }
}
