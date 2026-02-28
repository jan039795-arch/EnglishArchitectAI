using EA.Application.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EA.Application.Features.Assessments.GetAssessmentsByScope;

public class GetAssessmentsByScopeQueryHandler : IRequestHandler<GetAssessmentsByScopeQuery, List<AssessmentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAssessmentsByScopeQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AssessmentDto>> Handle(GetAssessmentsByScopeQuery request, CancellationToken cancellationToken)
    {
        return await _context.Assessments
            .Where(a => a.ScopeType == request.ScopeType && a.ScopeId == request.ScopeId)
            .Select(a => new AssessmentDto(a.Id, a.ScopeType.ToString(), a.ScopeId, a.Title, a.PassScore, a.TimeLimitMinutes, a.CEFRAligned))
            .ToListAsync(cancellationToken);
    }
}
