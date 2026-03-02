using FluentValidation;

namespace EA.Application.Features.Lessons.SearchLessons;

public class SearchLessonsQueryValidator : AbstractValidator<SearchLessonsQuery>
{
    public SearchLessonsQueryValidator()
    {
        RuleFor(x => x.Term)
            .NotEmpty()
            .MinimumLength(2);
    }
}
