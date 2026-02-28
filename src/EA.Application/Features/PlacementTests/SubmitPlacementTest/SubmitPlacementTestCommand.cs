using MediatR;

namespace EA.Application.Features.PlacementTests.SubmitPlacementTest;

public record PlacementAnswer(Guid? ExerciseId, string? Answer, bool IsCorrect);

public record SubmitPlacementTestCommand(
    string UserId,
    List<PlacementAnswer> Answers) : IRequest<string>;
