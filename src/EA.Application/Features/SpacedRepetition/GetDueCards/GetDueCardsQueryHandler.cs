using EA.Application.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EA.Application.Features.SpacedRepetition.GetDueCards;

public class GetDueCardsQueryHandler : IRequestHandler<GetDueCardsQuery, List<SpacedRepetitionCardDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDueCardsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SpacedRepetitionCardDto>> Handle(GetDueCardsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        return await _context.SpacedRepetitionCards
            .Where(c => c.UserId == request.UserId && c.NextReviewDate <= now)
            .Include(c => c.Exercise)
            .Take(request.MaxCards)
            .Select(c => new SpacedRepetitionCardDto(
                c.Id, c.UserId, c.ExerciseId,
                c.Exercise.Prompt, c.Exercise.Type.ToString(),
                c.EasinessFactor, c.Interval, c.Repetitions, c.NextReviewDate))
            .ToListAsync(cancellationToken);
    }
}
