using EA.Application.Contracts;
using EA.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EA.Application.Features.Exercises.SubmitResponse;

public class SubmitResponseCommandHandler : IRequestHandler<SubmitResponseCommand, SubmitResponseResult>
{
    private readonly IApplicationDbContext _context;
    private const double Alpha = 0.3;

    public SubmitResponseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SubmitResponseResult> Handle(SubmitResponseCommand request, CancellationToken cancellationToken)
    {
        var exercise = await _context.Exercises
            .FirstOrDefaultAsync(e => e.Id == request.ExerciseId, cancellationToken)
            ?? throw new InvalidOperationException($"Exercise {request.ExerciseId} not found.");

        var isCorrect = string.Equals(exercise.CorrectAnswer.Trim(), request.UserAnswer.Trim(), StringComparison.OrdinalIgnoreCase);

        var userResponse = new UserResponse
        {
            UserId = request.UserId,
            ExerciseId = request.ExerciseId,
            UserAnswer = request.UserAnswer,
            IsCorrect = isCorrect,
            TimeTakenSeconds = request.TimeTakenSeconds
        };
        _context.UserResponses.Add(userResponse);

        // Upsert UserWeakness per tag (EWMA α=0.3)
        if (!string.IsNullOrWhiteSpace(exercise.Tags))
        {
            var tags = exercise.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            double score = isCorrect ? 0.0 : 1.0;

            foreach (var tag in tags)
            {
                var weakness = await _context.UserWeaknesses
                    .FirstOrDefaultAsync(w => w.UserId == request.UserId && w.Tag == tag, cancellationToken);

                if (weakness is null)
                {
                    weakness = new UserWeakness
                    {
                        UserId = request.UserId,
                        Tag = tag,
                        WeaknessScore = score,
                        TotalAttempts = 1,
                        FailCount = isCorrect ? 0 : 1,
                        LastUpdated = DateTime.UtcNow
                    };
                    _context.UserWeaknesses.Add(weakness);
                }
                else
                {
                    weakness.WeaknessScore = Alpha * score + (1 - Alpha) * weakness.WeaknessScore;
                    weakness.TotalAttempts++;
                    if (!isCorrect) weakness.FailCount++;
                    weakness.LastUpdated = DateTime.UtcNow;
                }
            }
        }

        // Create SpacedRepetitionCard if not exists
        var cardExists = await _context.SpacedRepetitionCards
            .AnyAsync(c => c.UserId == request.UserId && c.ExerciseId == request.ExerciseId, cancellationToken);

        if (!cardExists)
        {
            _context.SpacedRepetitionCards.Add(new SpacedRepetitionCard
            {
                UserId = request.UserId,
                ExerciseId = request.ExerciseId,
                EasinessFactor = 2.5,
                Interval = 0,
                Repetitions = 0,
                NextReviewDate = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new SubmitResponseResult(isCorrect, null);
    }
}
