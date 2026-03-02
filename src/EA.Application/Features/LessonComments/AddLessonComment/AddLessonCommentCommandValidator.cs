using FluentValidation;

namespace EA.Application.Features.LessonComments.AddLessonComment;

public class AddLessonCommentCommandValidator : AbstractValidator<AddLessonCommentCommand>
{
    public AddLessonCommentCommandValidator()
    {
        RuleFor(x => x.LessonId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Body).NotEmpty().MaximumLength(500);
    }
}
