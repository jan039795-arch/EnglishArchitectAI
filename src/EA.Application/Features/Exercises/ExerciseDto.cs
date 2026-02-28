namespace EA.Application.Features.Exercises;

public record ExerciseOptionDto(Guid Id, string Text, string? Explanation);

public record ExerciseDto(
    Guid Id,
    Guid LessonId,
    string Type,
    string Prompt,
    int Difficulty,
    string? Tags,
    string Source,
    List<ExerciseOptionDto> Options);

public record SubmitResponseResult(bool IsCorrect, string? AIFeedback);
