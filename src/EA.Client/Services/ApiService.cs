using System.Net.Http.Json;

namespace EA.Client.Services;

// ── DTOs ─────────────────────────────────────────────────────────────────────

public record LevelDto(Guid Id, string Code, string Name, int Order, string? UnlockRequirement);
public record ModuleDto(Guid Id, Guid LevelId, string Title, string Description, int Order, string? YoutubePlaylistId, int EstimatedHours);
public record LessonDto(Guid Id, Guid ModuleId, string Title, string SkillType, int Order, bool IsAIGenerated);
public record LessonDetailDto(Guid Id, Guid ModuleId, string Title, string SkillType, int Order, bool IsAIGenerated, string? ContentJson);
public record ExerciseOptionDto(Guid Id, string Text, string? Explanation);
public record ExerciseDto(Guid Id, Guid LessonId, string Type, string Prompt, int Difficulty, string? Tags, string Source, List<ExerciseOptionDto> Options);
public record SubmitResponseResult(bool IsCorrect, string? AIFeedback);
public record UserProgressDto(Guid Id, string UserId, Guid LessonId, string LessonTitle, DateTime CompletedAt, int Score, int TimeSpentSeconds);
public record PlacementTestStatusDto(bool Completed, string? AssignedLevel, DateTime? CompletedAt);
public record PlacementAnswer(Guid? ExerciseId, string? Answer, bool IsCorrect);
public record SpacedRepetitionCardDto(Guid Id, string UserId, Guid ExerciseId, string ExercisePrompt, string ExerciseType, double EasinessFactor, int Interval, int Repetitions, DateTime NextReviewDate);

// ── Service ───────────────────────────────────────────────────────────────────

public class ApiService
{
    private readonly HttpClient _http;

    public ApiService(HttpClient http) => _http = http;

    // Levels
    public Task<List<LevelDto>?> GetLevelsAsync() =>
        _http.GetFromJsonAsync<List<LevelDto>>("api/levels");

    public Task<LevelDto?> GetLevelByIdAsync(Guid id) =>
        _http.GetFromJsonAsync<LevelDto>($"api/levels/{id}");

    // Modules
    public Task<List<ModuleDto>?> GetModulesByLevelAsync(Guid levelId) =>
        _http.GetFromJsonAsync<List<ModuleDto>>($"api/modules/by-level/{levelId}");

    // Lessons
    public Task<List<LessonDto>?> GetLessonsByModuleAsync(Guid moduleId) =>
        _http.GetFromJsonAsync<List<LessonDto>>($"api/lessons/by-module/{moduleId}");

    public Task<LessonDetailDto?> GetLessonByIdAsync(Guid id) =>
        _http.GetFromJsonAsync<LessonDetailDto>($"api/lessons/{id}");

    // Exercises
    public Task<List<ExerciseDto>?> GetExercisesByLessonAsync(Guid lessonId) =>
        _http.GetFromJsonAsync<List<ExerciseDto>>($"api/exercises/by-lesson/{lessonId}");

    public async Task<SubmitResponseResult?> SubmitResponseAsync(Guid exerciseId, string answer, int timeTakenSeconds)
    {
        var response = await _http.PostAsJsonAsync("api/exercises/submit",
            new { ExerciseId = exerciseId, UserAnswer = answer, TimeTakenSeconds = timeTakenSeconds });
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<SubmitResponseResult>();
    }

    // Progress
    public Task<List<UserProgressDto>?> GetUserProgressAsync() =>
        _http.GetFromJsonAsync<List<UserProgressDto>>("api/progress");

    public async Task<Guid?> CompleteLessonAsync(Guid lessonId, int score, int timeSpentSeconds)
    {
        var response = await _http.PostAsJsonAsync("api/progress/complete-lesson",
            new { LessonId = lessonId, Score = score, TimeSpentSeconds = timeSpentSeconds });
        if (!response.IsSuccessStatusCode) return null;
        var result = await response.Content.ReadFromJsonAsync<IdResponse>();
        return result?.Id;
    }

    // Placement
    public Task<PlacementTestStatusDto?> GetPlacementStatusAsync() =>
        _http.GetFromJsonAsync<PlacementTestStatusDto>("api/placementtest/status");

    public async Task<string?> SubmitPlacementTestAsync(List<PlacementAnswer> answers)
    {
        var response = await _http.PostAsJsonAsync("api/placementtest/submit", new { Answers = answers });
        if (!response.IsSuccessStatusCode) return null;
        var result = await response.Content.ReadFromJsonAsync<AssignedLevelResponse>();
        return result?.AssignedLevel;
    }

    // Spaced Repetition
    public Task<List<SpacedRepetitionCardDto>?> GetDueCardsAsync(int maxCards = 20) =>
        _http.GetFromJsonAsync<List<SpacedRepetitionCardDto>>($"api/spacedrepetition/due?maxCards={maxCards}");

    public async Task<bool> ReviewCardAsync(Guid cardId, int quality)
    {
        var response = await _http.PostAsJsonAsync("api/spacedrepetition/review",
            new { CardId = cardId, Quality = quality });
        return response.IsSuccessStatusCode;
    }

    private record IdResponse(Guid Id);
    private record AssignedLevelResponse(string AssignedLevel);
}
