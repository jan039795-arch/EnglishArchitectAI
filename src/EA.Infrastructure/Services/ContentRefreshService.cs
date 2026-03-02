using EA.Application.Contracts;
using EA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EA.Infrastructure.Services;

public class ContentRefreshService
{
    private readonly IApplicationDbContext _context;
    private readonly ExerciseGeneratorService _generator;
    private readonly ILogger<ContentRefreshService> _logger;

    public ContentRefreshService(
        IApplicationDbContext context,
        ExerciseGeneratorService generator,
        ILogger<ContentRefreshService> logger)
    {
        _context = context;
        _generator = generator;
        _logger = logger;
    }

    public async Task RefreshDailyContentAsync()
    {
        try
        {
            _logger.LogInformation("Starting daily content refresh job...");

            // Obtén ejercicios que no fueron actualizados hoy
            var today = DateTime.UtcNow.Date;
            var exercisesToUpdate = await _context.Exercises
                .Where(e => !_context.ContentVersions
                    .Where(cv => cv.ExerciseId == e.Id && cv.CreatedAt.Date == today)
                    .Any())
                .Take(10) // Limita a 10 por ejecución para no sobrecargar
                .ToListAsync();

            if (exercisesToUpdate.Count == 0)
            {
                _logger.LogInformation("No exercises to refresh today.");
                return;
            }

            int successCount = 0;
            int failCount = 0;

            foreach (var exercise in exercisesToUpdate)
            {
                try
                {
                    // Genera variación
                    var difficulty = (int)exercise.Difficulty;
                    var newPrompt = _generator.GenerateVariation(exercise.Prompt, difficulty);

                    // Evita duplicados exactos
                    if (newPrompt == exercise.Prompt)
                    {
                        _logger.LogWarning($"Skipped exercise {exercise.Id}: no variation generated.");
                        continue;
                    }

                    // Obtén el siguiente número de versión
                    var lastVersion = await _context.ContentVersions
                        .Where(cv => cv.ExerciseId == exercise.Id)
                        .OrderByDescending(cv => cv.VersionNumber)
                        .FirstOrDefaultAsync();

                    int nextVersionNumber = (lastVersion?.VersionNumber ?? 0) + 1;

                    // Crea registro de versión
                    var changelog = _generator.GenerateChangeLog(exercise.Prompt, newPrompt);
                    var contentVersion = new ContentVersion
                    {
                        ExerciseId = exercise.Id,
                        VersionNumber = nextVersionNumber,
                        OriginalPrompt = exercise.Prompt,
                        GeneratedPrompt = newPrompt,
                        ChangeLog = changelog,
                        IsActive = true,
                        GeneratedAt = DateTime.UtcNow
                    };

                    _context.ContentVersions.Add(contentVersion);
                    successCount++;

                    _logger.LogInformation(
                        $"Generated variation for exercise {exercise.Id} (v{nextVersionNumber}): {changelog}");
                }
                catch (Exception ex)
                {
                    failCount++;
                    _logger.LogError(ex, $"Failed to generate variation for exercise {exercise.Id}");
                }
            }

            // Guarda todos los cambios
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                $"Daily content refresh completed: {successCount} success, {failCount} failed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Daily content refresh job failed.");
        }
    }

    public async Task<ContentVersion?> GetLatestVersionAsync(Guid exerciseId)
    {
        return await _context.ContentVersions
            .Where(cv => cv.ExerciseId == exerciseId && cv.IsActive)
            .OrderByDescending(cv => cv.VersionNumber)
            .FirstOrDefaultAsync();
    }

    public async Task RollbackToVersionAsync(Guid exerciseId, int versionNumber)
    {
        try
        {
            var version = await _context.ContentVersions
                .FirstOrDefaultAsync(cv => cv.ExerciseId == exerciseId && cv.VersionNumber == versionNumber);

            if (version == null)
            {
                _logger.LogWarning($"Version {versionNumber} not found for exercise {exerciseId}");
                return;
            }

            // Desactiva todas las versiones posteriores
            var newerVersions = await _context.ContentVersions
                .Where(cv => cv.ExerciseId == exerciseId && cv.VersionNumber > versionNumber)
                .ToListAsync();

            foreach (var v in newerVersions)
                v.IsActive = false;

            version.IsActive = true;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Rolled back exercise {exerciseId} to version {versionNumber}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Rollback failed for exercise {exerciseId}");
        }
    }

    public async Task<List<ContentVersion>> GetVersionHistoryAsync(Guid exerciseId)
    {
        return await _context.ContentVersions
            .Where(cv => cv.ExerciseId == exerciseId)
            .OrderByDescending(cv => cv.VersionNumber)
            .ToListAsync();
    }
}
