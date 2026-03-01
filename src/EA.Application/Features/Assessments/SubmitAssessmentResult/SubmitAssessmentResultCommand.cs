using MediatR;

namespace EA.Application.Features.Assessments.SubmitAssessmentResult;

public record AssessmentAnswer(Guid ExerciseId, string Answer);

public record SubmitAssessmentResultCommand(
    string UserId,
    Guid AssessmentId,
    List<AssessmentAnswer> Answers) : IRequest<Guid>;
