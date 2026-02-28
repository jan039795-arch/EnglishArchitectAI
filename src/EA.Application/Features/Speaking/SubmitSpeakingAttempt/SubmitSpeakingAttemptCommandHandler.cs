using EA.Application.Contracts;
using EA.Domain.Entities;
using MediatR;

namespace EA.Application.Features.Speaking.SubmitSpeakingAttempt;

public class SubmitSpeakingAttemptCommandHandler : IRequestHandler<SubmitSpeakingAttemptCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public SubmitSpeakingAttemptCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(SubmitSpeakingAttemptCommand request, CancellationToken cancellationToken)
    {
        var attempt = new SpeakingAttempt
        {
            UserId = request.UserId,
            ExerciseId = request.ExerciseId,
            AudioBlobUrl = request.AudioBlobUrl,
            TranscriptText = request.TranscriptText,
            AccuracyScore = request.AccuracyScore,
            ProsodyScore = request.ProsodyScore,
            CompletenessScore = request.CompletenessScore,
            OverallScore = request.OverallScore,
            FeedbackJson = request.FeedbackJson
        };

        _context.SpeakingAttempts.Add(attempt);
        await _context.SaveChangesAsync(cancellationToken);

        return attempt.Id;
    }
}
