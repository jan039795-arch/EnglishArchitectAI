using MediatR;

namespace EA.Application.Features.Exercises.SubmitResponse;

public record SubmitResponseCommand(
    string UserId,
    Guid ExerciseId,
    string UserAnswer,
    int TimeTakenSeconds) : IRequest<SubmitResponseResult>;
