using EA.Application.Contracts;
using EA.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EA.Application.Features.Assessments.SubmitAssessmentResult;

public class SubmitAssessmentResultCommandHandler : IRequestHandler<SubmitAssessmentResultCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public SubmitAssessmentResultCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(SubmitAssessmentResultCommand request, CancellationToken cancellationToken)
    {
        var assessment = await _context.Assessments
            .FirstOrDefaultAsync(a => a.Id == request.AssessmentId, cancellationToken)
            ?? throw new InvalidOperationException($"Assessment {request.AssessmentId} not found.");

        var result = new AssessmentResult
        {
            UserId = request.UserId,
            AssessmentId = request.AssessmentId,
            Score = request.Score,
            IsPassed = request.Score >= assessment.PassScore,
            CompletedAt = DateTime.UtcNow
        };

        _context.AssessmentResults.Add(result);
        await _context.SaveChangesAsync(cancellationToken);

        return result.Id;
    }
}
