using EA.Application.Contracts;
using EA.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EA.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Level> Levels => Set<Level>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<ExerciseOption> ExerciseOptions => Set<ExerciseOption>();
    public DbSet<UserProgress> UserProgresses => Set<UserProgress>();
    public DbSet<UserWeakness> UserWeaknesses => Set<UserWeakness>();
    public DbSet<UserResponse> UserResponses => Set<UserResponse>();
    public DbSet<SpacedRepetitionCard> SpacedRepetitionCards => Set<SpacedRepetitionCard>();
    public DbSet<Assessment> Assessments => Set<Assessment>();
    public DbSet<AssessmentResult> AssessmentResults => Set<AssessmentResult>();
    public DbSet<SpeakingAttempt> SpeakingAttempts => Set<SpeakingAttempt>();
    public DbSet<PlacementTest> PlacementTests => Set<PlacementTest>();
    public DbSet<Certificate> Certificates => Set<Certificate>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
