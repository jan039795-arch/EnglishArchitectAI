using EA.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EA.Application.Contracts;

public interface IApplicationDbContext
{
    DbSet<Level> Levels { get; }
    DbSet<Module> Modules { get; }
    DbSet<Lesson> Lessons { get; }
    DbSet<Exercise> Exercises { get; }
    DbSet<ExerciseOption> ExerciseOptions { get; }
    DbSet<UserProgress> UserProgresses { get; }
    DbSet<UserWeakness> UserWeaknesses { get; }
    DbSet<UserResponse> UserResponses { get; }
    DbSet<SpacedRepetitionCard> SpacedRepetitionCards { get; }
    DbSet<Assessment> Assessments { get; }
    DbSet<AssessmentResult> AssessmentResults { get; }
    DbSet<SpeakingAttempt> SpeakingAttempts { get; }
    DbSet<PlacementTest> PlacementTests { get; }
    DbSet<Certificate> Certificates { get; }
    DbSet<Subscription> Subscriptions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
