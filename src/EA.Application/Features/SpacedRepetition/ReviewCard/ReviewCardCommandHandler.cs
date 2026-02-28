using EA.Application.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EA.Application.Features.SpacedRepetition.ReviewCard;

public class ReviewCardCommandHandler : IRequestHandler<ReviewCardCommand>
{
    private readonly IApplicationDbContext _context;

    public ReviewCardCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ReviewCardCommand request, CancellationToken cancellationToken)
    {
        var card = await _context.SpacedRepetitionCards
            .FirstOrDefaultAsync(c => c.Id == request.CardId && c.UserId == request.UserId, cancellationToken)
            ?? throw new InvalidOperationException($"Card {request.CardId} not found.");

        int q = request.Quality;

        if (q < 3)
        {
            card.Repetitions = 0;
            card.Interval = 1;
        }
        else
        {
            card.Interval = card.Repetitions switch
            {
                0 => 1,
                1 => 6,
                _ => (int)Math.Round(card.Interval * card.EasinessFactor)
            };
            card.Repetitions++;
        }

        card.EasinessFactor = Math.Max(1.3, card.EasinessFactor + 0.1 - (5 - q) * (0.08 + (5 - q) * 0.02));
        card.NextReviewDate = DateTime.UtcNow.AddDays(card.Interval);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
