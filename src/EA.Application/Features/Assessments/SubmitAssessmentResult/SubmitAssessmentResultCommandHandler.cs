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

        // Validate answers against the database
        var exerciseIds = request.Answers.Select(a => a.ExerciseId).ToList();
        var exercises = await _context.Exercises
            .Where(e => exerciseIds.Contains(e.Id))
            .ToListAsync(cancellationToken);

        var totalAnswers = request.Answers.Count;
        var correctAnswers = 0;

        foreach (var answer in request.Answers)
        {
            var exercise = exercises.FirstOrDefault(e => e.Id == answer.ExerciseId);
            var isCorrect = exercise != null &&
                           string.Equals(exercise.CorrectAnswer, answer.Answer, StringComparison.OrdinalIgnoreCase);

            if (isCorrect) correctAnswers++;
        }

        // Calculate score as percentage
        var score = totalAnswers == 0 ? 0 : (correctAnswers * 100) / totalAnswers;

        var result = new AssessmentResult
        {
            UserId = request.UserId,
            AssessmentId = request.AssessmentId,
            Score = score,
            IsPassed = score >= assessment.PassScore,
            CompletedAt = DateTime.UtcNow
        };

        _context.AssessmentResults.Add(result);
        await _context.SaveChangesAsync(cancellationToken);

        return result.Id;
    }
}
