using MediatR;

namespace EA.Application.Features.Assessments.SubmitAssessmentResult;

public record SubmitAssessmentResultCommand(
    string UserId,
    Guid AssessmentId,
    int Score) : IRequest<Guid>;
