using FluentValidation;

namespace EA.Application.Features.Exercises.SubmitResponse;

public class SubmitResponseCommandValidator : AbstractValidator<SubmitResponseCommand>
{
    public SubmitResponseCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ExerciseId).NotEmpty();
        RuleFor(x => x.UserAnswer).NotEmpty();
        RuleFor(x => x.TimeTakenSeconds).GreaterThanOrEqualTo(0);
    }
}
