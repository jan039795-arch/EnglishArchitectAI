using System.Text.Json;
using EA.Application.Contracts;
using EA.Domain.Entities;
using EA.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EA.Application.Features.PlacementTests.SubmitPlacementTest;

public class SubmitPlacementTestCommandHandler : IRequestHandler<SubmitPlacementTestCommand, string>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public SubmitPlacementTestCommandHandler(IApplicationDbContext context, IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<string> Handle(SubmitPlacementTestCommand request, CancellationToken cancellationToken)
    {
        // Validate answers against the database
        var exerciseIds = request.Answers.Select(a => a.ExerciseId).ToList();
        var exercises = await _context.Exercises
            .Where(e => exerciseIds.Contains(e.Id))
            .ToListAsync(cancellationToken);

        var total = request.Answers.Count;
        var correct = 0;
        var validatedAnswers = new List<object>();

        foreach (var answer in request.Answers)
        {
            var exercise = exercises.FirstOrDefault(e => e.Id == answer.ExerciseId);
            var isCorrect = exercise != null &&
                           string.Equals(exercise.CorrectAnswer, answer.Answer, StringComparison.OrdinalIgnoreCase);

            if (isCorrect) correct++;

            validatedAnswers.Add(new { exerciseId = answer.ExerciseId, answer = answer.Answer, isCorrect });
        }

        double percentage = total == 0 ? 0 : (double)correct / total * 100;

        var assignedLevel = percentage switch
        {
            <= 16 => CEFRLevel.A1,
            <= 33 => CEFRLevel.A2,
            <= 50 => CEFRLevel.B1,
            <= 66 => CEFRLevel.B2,
            <= 83 => CEFRLevel.C1,
            _ => CEFRLevel.C2
        };

        var test = new PlacementTest
        {
            UserId = request.UserId,
            FinalLevel = assignedLevel,
            TotalQuestions = total,
            ResponsesJson = JsonSerializer.Serialize(validatedAnswers),
            CompletedAt = DateTime.UtcNow
        };
        _context.PlacementTests.Add(test);

        await _identityService.CompletePlacementAsync(request.UserId, assignedLevel);
        await _context.SaveChangesAsync(cancellationToken);

        return assignedLevel.ToString();
    }
}
