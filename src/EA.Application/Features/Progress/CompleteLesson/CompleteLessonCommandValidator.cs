using FluentValidation;

namespace EA.Application.Features.Progress.CompleteLesson;

public class CompleteLessonCommandValidator : AbstractValidator<CompleteLessonCommand>
{
    public CompleteLessonCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.LessonId).NotEmpty();
        RuleFor(x => x.Score).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TimeSpentSeconds).GreaterThanOrEqualTo(0);
    }
}
