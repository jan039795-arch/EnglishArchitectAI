namespace EA.Application.Features.Assessments;

public record AssessmentDto(Guid Id, string ScopeType, Guid ScopeId, string Title, int PassScore, int TimeLimitMinutes, bool CEFRAligned);
