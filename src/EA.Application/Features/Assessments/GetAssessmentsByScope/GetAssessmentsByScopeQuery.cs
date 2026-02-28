using EA.Domain.Enums;
using MediatR;

namespace EA.Application.Features.Assessments.GetAssessmentsByScope;

public record GetAssessmentsByScopeQuery(AssessmentScopeType ScopeType, Guid ScopeId) : IRequest<List<AssessmentDto>>;
