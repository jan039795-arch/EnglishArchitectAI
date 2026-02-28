using MediatR;

namespace EA.Application.Features.Speaking.SubmitSpeakingAttempt;

public record SubmitSpeakingAttemptCommand(
    string UserId,
    Guid ExerciseId,
    string? AudioBlobUrl,
    string? TranscriptText,
    double AccuracyScore,
    double ProsodyScore,
    double CompletenessScore,
    double OverallScore,
    string? FeedbackJson) : IRequest<Guid>;
