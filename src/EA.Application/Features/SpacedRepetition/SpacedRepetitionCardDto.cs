namespace EA.Application.Features.SpacedRepetition;

public record SpacedRepetitionCardDto(
    Guid Id,
    string UserId,
    Guid ExerciseId,
    string ExercisePrompt,
    string ExerciseType,
    double EasinessFactor,
    int Interval,
    int Repetitions,
    DateTime NextReviewDate);
