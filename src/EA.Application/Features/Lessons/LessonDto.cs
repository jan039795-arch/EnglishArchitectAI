namespace EA.Application.Features.Lessons;

public record LessonDto(Guid Id, Guid ModuleId, string Title, string SkillType, int Order, bool IsAIGenerated);

public record LessonDetailDto(Guid Id, Guid ModuleId, string Title, string SkillType, int Order, bool IsAIGenerated, string? ContentJson);
