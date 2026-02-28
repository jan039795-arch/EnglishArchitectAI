using FluentValidation;

namespace EA.Application.Features.SpacedRepetition.ReviewCard;

public class ReviewCardCommandValidator : AbstractValidator<ReviewCardCommand>
{
    public ReviewCardCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.CardId).NotEmpty();
        RuleFor(x => x.Quality).InclusiveBetween(0, 5);
    }
}
