using FluentValidation;

namespace EA.Application.Features.Speaking.SubmitSpeakingAttempt;

public class SubmitSpeakingAttemptCommandValidator : AbstractValidator<SubmitSpeakingAttemptCommand>
{
    public SubmitSpeakingAttemptCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ExerciseId).NotEmpty();
        RuleFor(x => x.AccuracyScore).InclusiveBetween(0, 100);
        RuleFor(x => x.ProsodyScore).InclusiveBetween(0, 100);
        RuleFor(x => x.CompletenessScore).InclusiveBetween(0, 100);
        RuleFor(x => x.OverallScore).InclusiveBetween(0, 100);
    }
}
